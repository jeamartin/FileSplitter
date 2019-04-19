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
        public string Protocol
        {
            get { return "Shamirs"; }
        }

        static BigInteger RND_UPPER = BigInteger.Pow(2, 1023) - 1;//524287;//65535;//2147483647; //as secure as 32 bit secure :/ 

        static int SECRET_BYTE_SIZE = RND_UPPER.ToByteArray().Length;
        static int READ_BYTE_SIZE = SECRET_BYTE_SIZE-1; //Sans le -1 on arrive sur une imprecision qui rend le résultat fautx
        void test()
        {
            const int PARTS = 3;
            BigInteger secret = 123456;
            BigInteger[] polys = new BigInteger[PARTS];
            BigInteger[] shares = new BigInteger[PARTS];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();

            polys[0] = secret;
            for (int inum = 1; inum < polys.Length; inum++)
                polys[inum] = RandomInRange(rng, 0, RND_UPPER);

            for (byte j = 1; j <= PARTS; j++)
                shares[j-1] = evalPoly(polys, j);

            shares = new BigInteger[] {BigInteger.Parse("337300"),
                BigInteger.Parse("248186"),
                BigInteger.Parse("380401")};

            BigInteger retrivedSecret = lagrangeInterpolate(shares);

            Console.WriteLine("retrivedSecret" + retrivedSecret);
        }


        public void Merge(string target, string source, Type readType, Type writeType)
        {
            //test(); 
            //Console.WriteLine("<divmod>" + divmod(345698698, 10342456, RND_UPPER));
            //Console.WriteLine("c#special" + BigInteger.ModPow(345698698, 10342456, RND_UPPER));


            if (!readType.GetInterfaces().Contains(typeof(IGenReader)))
                throw new Exception("Incompatible type reader");
            if (!writeType.GetInterfaces().Contains(typeof(IGenWriter)))
                throw new Exception("Incompatible type writer");

            byte[] header = new byte[10];
            //1déterminer le nombre de partie en lisant le header du fichier source
            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                if (reader.Read(ref header, 10) < 10)
                {
                    throw new Exception("bad shrd file format.");
                }
                byte numberOfPart = getQtyTotal(header);
                byte indexOfSource = getIndex(header);
                long srcLength = getLengthFromHeader(header);

                //2vérifier la présence des fichiers sources nécessaires
                for (byte i = 0; i < numberOfPart; i++)
                {
                    string fileFullPath = getFileNameByIndex(source, i);
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
                        if (i != indexOfSource)
                        {
                            readers[i].Open(getFileNameByIndex(source, i));
                            readers[i].Read(ref header, 10);
                        }
                    //5instancier le writer nécessaire (target)
                    using (var writer = (IGenWriter)Activator.CreateInstance(writeType))
                    {
                        writer.Open(target);
                        bool oneEndOfFile = false;

                        byte[] byteBuffer = new byte[SECRET_BYTE_SIZE];
                        long writedLength = 0;
                        while (!oneEndOfFile)
                        {
                            BigInteger[] shares = new BigInteger[numberOfPart];
                            //6 retrouver STORE_BYTE_SIZE byte de toutes les parties
                            for (byte i = 0; i < numberOfPart; i++)
                            {
                                if (readers[i].Read(ref byteBuffer, SECRET_BYTE_SIZE) > 0) // take care of endianess here 
                                    shares[i] = new BigInteger(byteBuffer);
                                else
                                    oneEndOfFile = true;
                            }
                            if (!oneEndOfFile)
                            {
                                BigInteger bi = lagrangeInterpolate(shares);
                                //byte[] recover = oneByteArray((byte)(bi));//bi = {9223372036854775746}
                                //byte[] recover = BitConverter.GetBytes((UInt32)bi);
                                byte[] recover = bi.ToByteArray();

                                int length2Write = READ_BYTE_SIZE;

                                if (writedLength + READ_BYTE_SIZE > srcLength) //avoid writing extra bit if not necessary
                                    length2Write = (int)(srcLength - writedLength);

                                byte[] buffer = new byte[READ_BYTE_SIZE];

                                if (recover.Length == buffer.Length)
                                    buffer = recover;
                                else //the array may be to long (because of sign bit) or to short (eof)
                                    Array.Copy(recover, buffer, recover.Length>READ_BYTE_SIZE?READ_BYTE_SIZE:recover.Length);
                                
                                                                
                                writer.Write(buffer, length2Write);
                                writedLength += length2Write;// recover.Length;
                                //byte[] wr = bi.ToByteArray();
                                //writer.Write(wr, wr.Length);
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
        BigInteger lagrangeInterpolate(BigInteger[] shares)
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

            den = product(dens.ToArray());
            //Console.WriteLine("den->" + den);
            num = 0;
            for (int i = 0; i < shares.Length; i++)
            {
                num += divmod(MathMod(nums[i] * den * shares[i], RND_UPPER), dens[i], RND_UPPER);
            }
            //Console.WriteLine("num->" + num);

            return MathMod(divmod(num, den, RND_UPPER) + RND_UPPER, RND_UPPER);
        }

        /// <summary>
        /// Products of an array of value.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>product</returns>
        BigInteger product(BigInteger[] values)
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
                //long quot = (long)(Math.Floor((decimal)a / (decimal)b));

                BigInteger quot = FlooredBigIntDiv(a, b);
                //quot = quot != 0 ? (quot % quot - 10000000000000) / 10000000000000: 0;//new BigInteger(Math.Floor((decimal)a / (decimal)b)); // new BigInteger(Math.Floor( (double)a  / (double)b));
                //Console.WriteLine("a,b->" + a + " // " + b);
                //Console.WriteLine("quot1->" + quot);
                //quot = new BigInteger(Math.Floor((decimal)a / (decimal)b));
                //Console.WriteLine("quot2->" + quot);
                BigInteger tmp = MathMod(a, b);
                a = b;
                b = tmp;
                //Console.WriteLine("a,b->" + a + ", " + b);
                tmp = x;
                x = last_x - quot * x;
                last_x = tmp;
                //Console.WriteLine("x,last_x->" + x + ", " + last_x);
                tmp = y;
                y = last_y - quot * y;
                last_y = tmp;
                //Console.WriteLine("y,last_y->" + y + ", " + last_y);
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
        public void Shred(string source, Type readType, Type writeType, byte numberOfPart)
        {
            if (!readType.GetInterfaces().Contains(typeof(IGenReader)))
                throw new Exception("Incompatible type reader");
            if (!writeType.GetInterfaces().Contains(typeof(IGenWriter)))
                throw new Exception("Incompatible type writer");

            var writer = new IGenWriter[numberOfPart];
            string targetFolder = Path.GetDirectoryName(source);
            long sourceLength = (new FileInfo(source)).Length; // a déplacer dans une méthode non couplée (ExtFile)
            byte[] curRead = new byte[READ_BYTE_SIZE];

            RandomNumberGenerator rng = RandomNumberGenerator.Create();//shouldn't be pseudo random...

            BigInteger[] polys = new BigInteger[numberOfPart];
            //polys[0] = 0;//secret place

            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                try
                {
                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        writer[i] = (IGenWriter)Activator.CreateInstance(writeType);
                        writer[i].Open(writeFileName(source, targetFolder, i));
                    }
                    //écriture des metainfos en header.
                    for (byte i = 0; i < numberOfPart; i++)
                        writer[i].Write(getHeader(sourceLength, numberOfPart, i), 10);

                    int qtyRead = 0;

                    while ((qtyRead = reader.Read(ref curRead, READ_BYTE_SIZE)) > 0)
                    {
                        byte[] ubytes = new byte[READ_BYTE_SIZE+1];//il faut lire un nombre entier positif.
                        curRead.CopyTo(ubytes, 0);
                        ubytes[READ_BYTE_SIZE] = 0;
                        polys[0] = new BigInteger(ubytes);//BitConverter.ToUInt32(curRead, 0);//curRead[i];//new BigInteger(curRead); 
                        for (int inum = 1; inum < polys.Length; inum++)
                            polys[inum] = RandomInRange(rng, 0, RND_UPPER);


                        for (byte j = 0; j < numberOfPart; j++)
                        {
                            BigInteger ev = evalPoly(polys, j + 1);
                            /*for (int k = 0; k < polys.Length; k++)
                                Console.Write(polys[k] + "|");
                            Console.WriteLine(ev);*/

                            byte[] share = ev.ToByteArray(); // BitConverter.GetBytes(ev);

                            byte[] paddedShare = new byte[SECRET_BYTE_SIZE];

                            share.CopyTo(paddedShare, 0);
                            //if (share.Length != STORE_BYTE_SIZE)
                            //    Console.WriteLine("not 16 bytes bigint detected >" + new BigInteger(share) +"><"+ new BigInteger(paddedShare));

                            writer[j].Write(paddedShare, paddedShare.Length);
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

        byte[] oneByteArray(byte b)
        {
            byte[] ret = new byte[1];
            ret[0] = b;
            return ret;
        }

        string writeFileName(string source, string targetFolder, byte index)
        {
            string ret = (targetFolder.LastIndexOf(@"\") == targetFolder.Length - 1 ? targetFolder : targetFolder + Path.DirectorySeparatorChar) +
                Path.GetFileName(source) + "." + (index).ToString().PadLeft(3, '0') + "." + FileSplitterCommon.FILE_EXT;
            return ret;
        }

        string getFileNameByIndex(string source, byte index)
        {
            return source.Substring(0, source.Length - 8) + (index).ToString().PadLeft(3, '0') + "." + FileSplitterCommon.FILE_EXT;
        }

        long getLengthFromHeader(byte[] header)
        {
            return BitConverter.ToInt64(header, 0);
        }
        byte getIndex(byte[] header)
        {
            return header[8];
        }

        byte getQtyTotal(byte[] header)
        {
            return header[9];
        }

        byte[] getHeader(long sourceLength, byte numberOfPart, byte index)
        {
            byte[] length = BitConverter.GetBytes(sourceLength);

            byte[] ret = new byte[10];

            length.CopyTo(ret, 0);
            ret[8] = index;
            ret[9] = numberOfPart;
            return ret;
        }

        BigInteger evalPoly(BigInteger[] polys, BigInteger x)
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
