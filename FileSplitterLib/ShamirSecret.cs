using FileSplitterCommon;
using FileSplitterDef;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileSplitterLib
{
    [Export(typeof(IFileMerger))]
    [Export(typeof(IFileSpliter))]
    [PartCreationPolicy(CreationPolicy.Shared)]

    // From https://en.wikipedia.org/wiki/Shamir%27s_Secret_Sharing
    public class ShamirSecret : IFileSpliter, IFileMerger
    {
        public string UserName
        {
            get { return "Shamirs"; }
        }

        public Guid Protocol
        {
            get { return new Guid("900CB619-7663-4DC6-96EA-DA5BE11860A9"); }
        }

        static BigInteger RND_UPPER = BigInteger.Pow(2, 1023) - 1;//524287;//65535;//2147483647; //as secure as 32 bit secure :/ 

        static int SECRET_BYTE_SIZE = RND_UPPER.ToByteArray().Length;
        static int READ_BYTE_SIZE = SECRET_BYTE_SIZE-1; //Sans le -1 on arrive sur une imprecision qui rend le résultat faux


        public void Merge(Stream target, Stream[] sources, /*Header header,*/ int position, byte numberOfPart, long totalLength)
        {
            if (sources.Length < 2)
                throw new ArgumentException("You need at least 2 sources");

            //ensure all the source stream positions are after the header.
            foreach (var source in sources)
                if (source.Position < position)
                    source.Position = position;

            bool oneEndOfFile = false;

            byte[] byteBuffer = new byte[SECRET_BYTE_SIZE];
            long writedLength = 0;
            while (!oneEndOfFile)
            {
                BigInteger[] shares = new BigInteger[numberOfPart];
                // retrouver STORE_BYTE_SIZE byte de toutes les parties
                for (byte i = 0; i < numberOfPart; i++)
                {
                    if (sources[i].Read(byteBuffer, 0, SECRET_BYTE_SIZE) > 0) // take care of endianess here 
                        shares[i] = new BigInteger(byteBuffer);
                    else
                        oneEndOfFile = true;
                }
                if (!oneEndOfFile)
                {
                    BigInteger bi = LagrangeInterpolate(shares);
                    byte[] recover = bi.ToByteArray();

                    int length2Write = READ_BYTE_SIZE;

                    if (writedLength + READ_BYTE_SIZE > totalLength) //avoid writing extra bit if not necessary
                        length2Write = (int)(totalLength - writedLength);

                    byte[] writeBuffer = new byte[READ_BYTE_SIZE];

                    if (recover.Length == READ_BYTE_SIZE)
                        writeBuffer = recover;
                    else //the array may be to long (because of sign bit) or to short (eof)
                        Array.Copy(recover, writeBuffer, recover.Length > READ_BYTE_SIZE ? READ_BYTE_SIZE : recover.Length);

                    target.Write(writeBuffer, 0, READ_BYTE_SIZE);

                    writedLength += length2Write;
                }
            }
        }
        public void Merge(Stream target, Stream[] sources, byte[] header)
        {
            if (sources.Length < 2)
                throw new ArgumentException("You need at least 2 sources");

            //ensure all the source stream positions are after the header.
            foreach (var source in sources)
                if (source.Position < header.Length)
                    source.Position = header.Length;

            byte numberOfPart = Utils.GetQtyTotal(header);
            byte indexOfSource = Utils.GetIndex(header);
            long srcLength = Utils.GetLengthFromHeader(header);
            bool oneEndOfFile = false;

            byte[] byteBuffer = new byte[SECRET_BYTE_SIZE];
            long writedLength = 0;
            while (!oneEndOfFile)
            {
                BigInteger[] shares = new BigInteger[numberOfPart];
                // retrouver STORE_BYTE_SIZE byte de toutes les parties
                for (byte i = 0; i < numberOfPart; i++)
                {
                    if (sources[i].Read(byteBuffer, 0, SECRET_BYTE_SIZE) > 0) // take care of endianess here 
                        shares[i] = new BigInteger(byteBuffer);
                    else
                        oneEndOfFile = true;
                }
                if (!oneEndOfFile)
                {
                    BigInteger bi = LagrangeInterpolate(shares);
                    byte[] recover = bi.ToByteArray();

                    int length2Write = READ_BYTE_SIZE;

                    if (writedLength + READ_BYTE_SIZE > srcLength) //avoid writing extra bit if not necessary
                        length2Write = (int)(srcLength - writedLength);

                    byte[] writeBuffer = new byte[READ_BYTE_SIZE];

                    if (recover.Length == READ_BYTE_SIZE)
                        writeBuffer = recover;
                    else //the array may be to long (because of sign bit) or to short (eof)
                        Array.Copy(recover, writeBuffer, recover.Length > READ_BYTE_SIZE ? READ_BYTE_SIZE : recover.Length);

                    target.Write(writeBuffer, 0, READ_BYTE_SIZE);

                    writedLength += length2Write;
                }
            }
        }


        public void Merge(string target, string source, Type readType, Type writeType)
        {
            if (!readType.GetInterfaces().Contains(typeof(IGenReader)))
                throw new Exception("Incompatible type reader");
            if (!writeType.GetInterfaces().Contains(typeof(IGenWriter)))
                throw new Exception("Incompatible type writer");

            //1déterminer le nombre de partie en lisant le header du fichier source
            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                reader.BufferSize = 10;
                if (reader.Read(10) < 10)
                {
                    throw new Exception("bad shrd file format.");
                }
                byte numberOfPart = Utils.GetQtyTotal(reader.Buffer);
                byte indexOfSource = Utils.GetIndex(reader.Buffer);
                long srcLength = Utils.GetLengthFromHeader(reader.Buffer);

                //2vérifier la présence des fichiers sources nécessaires
                for (byte i = 0; i < numberOfPart; i++)
                {
                    string fileFullPath = Utils.GetFileNameByIndex(source, i);
                    if (!File.Exists(fileFullPath))
                        throw new Exception("File not found" + fileFullPath);
                }
                //3instancier les readers nécessaires
                IGenReader[] readers = new IGenReader[numberOfPart];
                try
                {
                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        if (i == indexOfSource)
                            readers[i] = reader;
                        else
                            readers[i] = (IGenReader)Activator.CreateInstance(readType);
                    }
                    //4avancer le pointeur jusqu'à la fin de l'en-tête. 
                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        readers[i].BufferSize = SECRET_BYTE_SIZE;
                        if (i != indexOfSource)
                        {
                            readers[i].Open(Utils.GetFileNameByIndex(source, i));
                            readers[i].Read(10);
                        }
                    }
                    //5instancier le writer nécessaire (target)
                    using (var writer = (IGenWriter)Activator.CreateInstance(writeType))
                    {
                        writer.Open(target);
                        writer.BufferSize = READ_BYTE_SIZE;
                        bool oneEndOfFile = false;

                        //byte[] byteBuffer = new byte[SECRET_BYTE_SIZE];
                        long writedLength = 0;
                        while (!oneEndOfFile)
                        {
                            BigInteger[] shares = new BigInteger[numberOfPart];
                            //6 retrouver STORE_BYTE_SIZE byte de toutes les parties
                            for (byte i = 0; i < numberOfPart; i++)
                            {
                                if (readers[i].Read(SECRET_BYTE_SIZE) > 0) // take care of endianess here 
                                    shares[i] = new BigInteger(readers[i].Buffer);
                                else
                                    oneEndOfFile = true;
                            }
                            if (!oneEndOfFile)
                            {
                                BigInteger bi = LagrangeInterpolate(shares);
                                byte[] recover = bi.ToByteArray();

                                int length2Write = READ_BYTE_SIZE;

                                if (writedLength + READ_BYTE_SIZE > srcLength) //avoid writing extra bit if not necessary
                                    length2Write = (int)(srcLength - writedLength);

                                if (recover.Length == writer.Buffer.Length)
                                    writer.Buffer = recover;
                                else //the array may be to long (because of sign bit) or to short (eof)
                                    Array.Copy(recover, writer.Buffer, recover.Length>READ_BYTE_SIZE?READ_BYTE_SIZE:recover.Length);
                                                                
                                writer.Write(length2Write);

                                writedLength += length2Write;// recover.Length;
                            }
                        }
                    }
                }
                finally
                {
                    for (byte i = 0; i < numberOfPart; i++)
                        if (i != indexOfSource && readers[i] != null)
                            readers[i].Dispose();
                }
            }
        }

        /// <summary>
        /// Compute Lagrange's interpolate.
        /// </summary>
        /// <param name="shares">The shares.</param>
        /// <returns>secret</returns>
        BigInteger LagrangeInterpolate(BigInteger[] shares)
        {
            List<BigInteger> nums = new List<BigInteger>();
            List<BigInteger> dens = new List<BigInteger>();
            BigInteger num, den;

            for (int i = 0; i < shares.Length; i++)
            {
                num = 1;
                den = 1;
                for (int j = 0; j < shares.Length; j++)
                {
                    if (j != i)
                    {
                        num *= 0 - (j+1);
                        den *= (i+1) - (j+1);
                    }
                }
                nums.Add(num);
                dens.Add(den);
            }

            den = Product(dens.ToArray());
            num = 0;
            for (int i = 0; i < shares.Length; i++)
            {
                num += divmod(MathMod(nums[i] * den * shares[i], RND_UPPER), dens[i], RND_UPPER);
            }

            return MathMod(divmod(num, den, RND_UPPER) + RND_UPPER, RND_UPPER);
        }

        /// <summary>
        /// Products of an array of value.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>product</returns>
        BigInteger Product(BigInteger[] values)
        {
            BigInteger ret = 1;
            foreach (var val in values)
                ret *= val;
            return ret;
        }

        /// <summary>
        /// Divmods GDC * num
        /// </summary>
        /// <param name="num">The number.</param>
        /// <param name="den">The den.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>GDC * num</returns>
        BigInteger divmod(BigInteger num, BigInteger den, BigInteger max)
        {
            BigInteger gcd = findGCD(den, max);
            return num * gcd;
        }

        /// <summary>
        /// BigInteger div Floored  (same as // python operator) from https://stackoverflow.com/q/28059655/8165479
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>The floored division</returns>
        static BigInteger FlooredBigIntDiv(BigInteger a, BigInteger b)
        {
            if (a < 0)
            {
                if (b > 0)
                    return (a - b + 1) / b;
            }
            else if (a > 0)
            {
                if (b < 0)
                    return (a - b - 1) / b;
            }
            return a / b;
        }
        /// <summary>
        /// Finds the GCD.
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>the gdc</returns>
        public BigInteger findGCD(BigInteger a, BigInteger b)
        {
            BigInteger x = 0;
            BigInteger last_x = 1;
            BigInteger y = 1;
            BigInteger last_y = 0;
            while (b != 0)
            {
                BigInteger quot = FlooredBigIntDiv(a, b);
                BigInteger tmp = MathMod(a, b);
                a = b;
                b = tmp;
                tmp = x;
                x = last_x - quot * x;
                last_x = tmp;
                tmp = y;
                y = last_y - quot * y;
                last_y = tmp;
            }
            return last_x;
        }

        /// <summary>
        /// Compute the modulus and not the reminder, unlike % operator in c#, but similar to python % operator.
        /// </summary>
        /// <param name="a">a the numerator</param>
        /// <param name="b">b the denominator</param>
        /// <returns>Math Modulus of a/b</returns>
        static BigInteger MathMod(BigInteger a, BigInteger b)
        {
            return (((a % b) + b) % b);
        }

        public void Split(Stream[] targets, Stream source, byte numberOfPart)
        {
            byte[] buffer = new byte[READ_BYTE_SIZE];

            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            BigInteger[] polys = new BigInteger[numberOfPart];

            int qtyRead = 0;

            while ((qtyRead = source.Read(buffer, 0, READ_BYTE_SIZE)) > 0)
            {
                byte[] ubytes = new byte[READ_BYTE_SIZE + 1];//il faut lire un nombre entier positif.
                //byte[] writeBuffer = new byte[SECRET_BYTE_SIZE];
                buffer.CopyTo(ubytes, 0);
                ubytes[READ_BYTE_SIZE] = 0;
                polys[0] = new BigInteger(ubytes);//BitConverter.ToUInt32(curRead, 0);//curRead[i];//new BigInteger(curRead); 
                for (int inum = 1; inum < polys.Length; inum++)
                    polys[inum] = RandomInRange(rng, 0, RND_UPPER);

                for (byte j = 0; j < numberOfPart; j++)
                {
                    BigInteger ev = EvalPoly(polys, j + 1);
                    byte[] share = ev.ToByteArray(); // BitConverter.GetBytes(ev);

                    byte[] paddedShare = new byte[SECRET_BYTE_SIZE];

                    share.CopyTo(paddedShare, 0);
                    targets[j].Write(paddedShare, 0, SECRET_BYTE_SIZE);
                }
            }
        }

        public void Shred(string source, Type readType, Type writeType, byte numberOfPart)
        {
            if (!readType.GetInterfaces().Contains(typeof(IGenReader)))
                throw new Exception("Incompatible type reader");
            if (!writeType.GetInterfaces().Contains(typeof(IGenWriter)))
                throw new Exception("Incompatible type writer");

            var writer = new IGenWriter[numberOfPart];
            string targetFolder = Path.GetDirectoryName(source);
            long sourceLength = (new FileInfo(source)).Length; // a déplacer dans une méthode non couplée (ExtFile)
            //byte[] curRead = new byte[READ_BYTE_SIZE];

            RandomNumberGenerator rng = RandomNumberGenerator.Create();//shouldn't be pseudo random...

            BigInteger[] polys = new BigInteger[numberOfPart];
            //polys[0] = 0;//secret place

            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                reader.BufferSize = READ_BYTE_SIZE;
                try
                {
                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        writer[i] = (IGenWriter)Activator.CreateInstance(writeType);
                        writer[i].Open(Utils.WriteFileName(source, targetFolder, i));
                        writer[i].BufferSize = SECRET_BYTE_SIZE;
                    }
                    //écriture des metainfos en header.
                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        Array.Copy(Utils.GetHeader(sourceLength, numberOfPart, i), writer[i].Buffer, 10);
                        writer[i].Write(10);
                    }
                    int qtyRead = 0;

                    while ((qtyRead = reader.Read(READ_BYTE_SIZE)) > 0)
                    {
                        byte[] ubytes = new byte[READ_BYTE_SIZE+1];//il faut lire un nombre entier positif.
                        reader.Buffer.CopyTo(ubytes, 0);
                        ubytes[READ_BYTE_SIZE] = 0;
                        polys[0] = new BigInteger(ubytes);//BitConverter.ToUInt32(curRead, 0);//curRead[i];//new BigInteger(curRead); 
                        for (int inum = 1; inum < polys.Length; inum++)
                            polys[inum] = RandomInRange(rng, 0, RND_UPPER);


                        for (byte j = 0; j < numberOfPart; j++)
                        {
                            BigInteger ev = EvalPoly(polys, j + 1);
                            
                            byte[] share = ev.ToByteArray(); // BitConverter.GetBytes(ev);

                            for (int k = 0; k < writer[j].BufferSize; k++)
                                writer[j].Buffer[k] = 0;

                            share.CopyTo(writer[j].Buffer, 0);

                            writer[j].Write(writer[j].Buffer.Length);
                        }
                    }
                }
                finally
                {
                    for (byte i = 0; i < numberOfPart; i++)
                        if (writer != null)
                            writer[i].Dispose();
                }
            }

        }

        /// <summary>
        /// Create pseudo-random BigInteger. From https://stackoverflow.com/a/48855115/8165479
        /// </summary>
        /// <param name="rng">The RNG.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>A pseudo-random BigInteger</returns>
        BigInteger RandomInRange(RandomNumberGenerator rng, BigInteger min, BigInteger max)
        {
            if (min > max)
            {
                var buff = min;
                min = max;
                max = buff;
            }

            // offset to set min = 0
            BigInteger offset = -min;
            min = 0;
            max += offset;

            var value = randomInRangeFromZeroToPositive(rng, max) - offset;
            return value;
        }

        /// <summary>
        /// Create pseudo-random BigInteger from zero to positive.
        /// </summary>
        /// <param name="rng">The RNG.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>A pseudo-random BigInteger</returns>
        BigInteger randomInRangeFromZeroToPositive(RandomNumberGenerator rng, BigInteger max)
        {
            BigInteger value;
            var bytes = max.ToByteArray();

            // count how many bits of the most significant byte are 0
            // NOTE: sign bit is always 0 because `max` must always be positive
            byte zeroBitsMask = 0x00;

            var mostSignificantByte = bytes[bytes.Length - 1];

            // we try to set to 0 as many bits as there are in the most significant byte, starting from the left (most significant bits first)
            // NOTE: `i` starts from 7 because the sign bit is always 0
            for (var i = 7; i >= 0; i--)
            {
                // we keep iterating until we find the most significant non-0 bit
                if ((mostSignificantByte & (0x1 << i)) != 0)
                {
                    var zeroBits = 7 - i;
                    zeroBitsMask = (byte)(0xFF >> zeroBits);
                    break;
                }
            }

            do
            {
                rng.GetBytes(bytes);

                // set most significant bits to 0 (because `value > max` if any of these bits is 1)
                bytes[bytes.Length - 1] &= zeroBitsMask;

                value = new BigInteger(bytes);

                // `value > max` 50% of the times, in which case the fastest way to keep the distribution uniform is to try again
            } while (value > max);

            return value;
        }

        BigInteger EvalPoly(BigInteger[] polys, BigInteger x)
        {
            BigInteger ret = 0;

            for (int coeff = polys.Length - 1; coeff >= 0; coeff--)
            {
                ret *= x;
                ret += polys[coeff];
                ret %= RND_UPPER;
            }
            return ret;
        }
    }
}
