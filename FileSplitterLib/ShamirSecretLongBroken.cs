using RWDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExtFile
{
    [Export(typeof(IFileMerger))]
    [Export(typeof(IFileSpliter))]
    [PartCreationPolicy(CreationPolicy.Shared)]

    public class ShamirSecret : IFileSpliter, IFileMerger
    {
        public string Protocol
        {
            get { return "Shamirs"; }
        }

        static long RND_UPPER = 2305843009213693951; //Mersenne prime 9 Math.Pow(2, 61) - 1; long.Pow(2, 127) - 1;//524287;//65535;//2147483647; //as secure as 32 bit secure :/ 

        const int STORE_BYTE_SIZE = 8;

        void test()
        {
            const int PARTS = 3;
            long secret = 123456789101112;
            long[] polys = new long[PARTS];
            long[] shares = new long[PARTS];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();

            polys[0] = secret;
            for (int inum = 1; inum < polys.Length; inum++)
                polys[inum] = RandomInRange(rng, 0, RND_UPPER);

            for (byte j = 1; j <= PARTS; j++)
                shares[j-1] = evalPoly(polys, j);

            shares = new long[] {long.Parse("1742152079029678955"),
                long.Parse("80629544000053457"),
                long.Parse("1933084879341306471")};

            long retrivedSecret = lagrangeInterpolate(shares);

            Console.WriteLine("retrivedSecret" + retrivedSecret);
        }


        public void Merge(string target, string source, Type readType, Type writeType)
        {
            test(); 
            //Console.WriteLine("<divmod>" + divmod(345698698, 10342456, RND_UPPER));
            //Console.WriteLine("c#special" + long.ModPow(345698698, 10342456, RND_UPPER));


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
                long length = getLengthFromHeader(header);

                //2vérifier la présence des fichiers sources nécessaires
                for (byte i = 0; i < numberOfPart; i++)
                {
                    string fileFullPath = getFileNameByIndex(source, i);
                    if (!File.Exists(fileFullPath))
                        throw new Exception("File not found" + fileFullPath);
                }
                //3instancier les readers nécessaires
                IGenReader[] readers = new IGenReader[numberOfPart];
                for (byte i = 0; i < numberOfPart; i++)
                {
                    if (i == indexOfSource)
                        readers[i] = reader;
                    else
                        readers[i] = (IGenReader)Activator.CreateInstance(readType);
                }
                try
                {
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

                        byte[] byteBuffer = new byte[STORE_BYTE_SIZE];

                        while (!oneEndOfFile)
                        {
                            long[] shares = new long[numberOfPart];
                            //6 retrouver STORE_BYTE_SIZE byte de toutes les parties
                            for (byte i = 0; i < numberOfPart; i++)
                            {
                                if (readers[i].Read(ref byteBuffer, STORE_BYTE_SIZE) > 0) // take care of endianess here 
                                    shares[i] = BitConverter.ToInt64(byteBuffer, 0);
                                else
                                    oneEndOfFile = true;
                            }
                            if (!oneEndOfFile)
                            {
                                long bi = lagrangeInterpolate(shares);
                                byte recover = (byte)(bi % 255);
                                writer.Write(oneByteArray(recover), 1);
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

        long lagrangeInterpolate(long[] shares)
        {
            List<long> nums = new List<long>();
            List<long> dens = new List<long>();
            long num, den;

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
            Console.WriteLine("den->" + den);
            num = 0;
            for (int i = 0; i < shares.Length; i++)
            {
                num += divmod(MathMod(nums[i] * den * shares[i], RND_UPPER), dens[i], RND_UPPER);
            }
            Console.WriteLine("num->" + num);

            return MathMod(divmod(num, den, RND_UPPER) + RND_UPPER, RND_UPPER);
            //num = 
        }
        long product(long[] values)
        {
            long ret = 1;
            foreach (var val in values)
                ret *= val;
            return ret;
        }

        long divmod(long num, long den, long max)
        {
            long gcd = findGCD(den, max);
            return num * gcd;
        }
        public long findGCD(long a, long b)
        {
            long x = 0;
            long last_x = 1;
            long y = 1;
            long last_y = 0;
            while (b != 0)
            {
                decimal result = (decimal)a / (decimal)b;

                //Console.WriteLine("result->" + result + "Floor :>" + Math.Floor(result));

                long quot = (long)(Math.Floor((decimal)a / (decimal)b));
                Console.WriteLine("a,b->" + a + " // " + b);
                Console.WriteLine("quot->" + quot);
                long tmp = MathMod(a, b);
                a = b;
                b = tmp;
                Console.WriteLine("a,b<|" + a + ", " + b);
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

        /*float nfmod(float a, float b)
        {
            return a - b * floor(a / b)
        }*/

        static long MathMod(long a, long b)
        {
            //Math.DivRem
            return (((a % b) + b) % b);
            //return ((a % b) + b) % b;//(long)(a - b * Math.Floor((decimal)a / (decimal)b));
            //return (Math.Abs(a * b) + a) % b;
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
            byte[] curRead = new byte[1024];

            RandomNumberGenerator rng = RandomNumberGenerator.Create();//shouldn't be pseudo random...

            long[] polys = new long[numberOfPart];
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

                    while ((qtyRead = reader.Read(ref curRead, 1024)) > 0)
                    {
                        for (int i = 0; i < qtyRead; i++)
                        {
                            polys[0] = curRead[i];
                            for (int inum = 1; inum < polys.Length; inum++)
                                polys[inum] = RandomInRange(rng, 0, RND_UPPER);


                            for (byte j = 0; j < numberOfPart; j++)
                            {
                                long ev = evalPoly(polys, j + 1);
                                /*for (int k = 0; k < polys.Length; k++)
                                    Console.Write(polys[k] + "|");
                                Console.WriteLine(ev);*/

                                byte[] share = BitConverter.GetBytes(ev); //Bitev.ToByteArray(); // BitConverter.GetBytes(ev);

                                //byte[] paddedShare = new byte[STORE_BYTE_SIZE];

                                //share.CopyTo(paddedShare, 0);
                                //if (share.Length != STORE_BYTE_SIZE)
                                //    Console.WriteLine("not 16 bytes bigint detected >" + new long(share) +"><"+ new long(paddedShare));

                                writer[j].Write(share, share.Length);
                            }
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

        long RandomInRange(RandomNumberGenerator rng, long min, long max)
        {
            if (min > max)
            {
                var buff = min;
                min = max;
                max = buff;
            }

            // offset to set min = 0
            long offset = -min;
            min = 0;
            max += offset;

            var value = randomInRangeFromZeroToPositive(rng, max) - offset;
            return value;
        }

        long randomInRangeFromZeroToPositive(RandomNumberGenerator rng, long max)
        {
            long value;
            var bytes = BitConverter.GetBytes(max);// max.ToByteArray();

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

                value = BitConverter.ToInt64(bytes, 0); // new long(bytes);

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
                Path.GetFileName(source) + ".shasec" + (index).ToString().PadLeft(3, '0');
            return ret;
        }

        string getFileNameByIndex(string source, byte index)
        {
            return source.Substring(0, source.Length - 3) + (index).ToString().PadLeft(3, '0');
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

        long evalPoly(long[] polys, long x)
        {
            long ret = 0;

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
