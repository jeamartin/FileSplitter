using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
header (intel convention) v1: 
A. File preambule (split)        5  bytes
B. File format version           2  bytes -> 1
C. Total Header size             2  bytes
D. Part index			         1  byte
E. Total part count              1  byte
F. Format de split 	             16 bytes (1 slice, 2 confetti, 3 shamir1024)
G. Original file length          8  bytes
H. Part file name length         2  bytes (sometime filename can be longer than 255)
I. Format de hash				 16 bytes (00000000-0000-0000-0000-000000000000) mean no hash (1 = tbd)
J. Part file hash length         2  bytes (0 no hash)
K. File name or part             H, if part -> significante part -> size = H / E * (F overhead)
L. Hash or hash part             J, if part -> significante part -> size = J / E * (F overhead)
M. File part					 variable size = G / E * (F overhead)

header (intel convention) v1: 
A. File preambule (split)        5  bytes
B. File format version           2  bytes -> 1
C. Total Header size             2  bytes
D. Part index			         1  byte
E. Total part count              1  byte
F. Format de split 	             16 bytes (1 slice, 2 confetti, 3 shamir1024)
G. Original file length          8  bytes
H. Part file name length         2  bytes 
I. Total file name length        2  bytes (sometime filename can be longer than 255)
J. Format de hash				 16 bytes (00000000-0000-0000-0000-000000000000) mean no hash (1 = tbd)
K. Part file hash length         2  bytes (0 no hash)
L. Total file hash length        2  bytes (0 no hash)
M. File name or part             H, if part -> significante part -> size = I / E * (F overhead)
N. Hash or hash part             K, if part -> significante part -> size = L / E * (F overhead)
O. File part					 variable size = G / E * (F overhead)
*/

namespace FileSplitterCommon
{
    public class Header
    {
        public const ushort FirstReadSize = 59;
        public const string FilePreambule = "split";
        public const ushort FileFormatVersion = 1;

        public ushort TotalHeaderSize => ((ushort)(FirstReadSize + PartFileNameLength + PartFileHashLength));

        public byte PartIndex;
        public byte TotalPartCount;
        public Guid SplitFormat = new Guid();
        public long OriginalFileLength;
        public ushort PartFileNameLength;
        public ushort TotalFileNameLength;
        public byte[] FileName;
        public Guid HashFormat = new Guid();
        public ushort PartFileHashLength = 0;
        public ushort TotalFileHashLength = 0;
        public byte[] Hash;

        public static Header CreateHeader(Stream stream)
        {
            var buffer = new byte[Header.FirstReadSize];

            if (stream.Read(buffer, 0, Header.FirstReadSize) != Header.FirstReadSize)
                throw new Exception("Header not long enough");

            Header ret = Header.CreateHeader(buffer);

            ret.FileName = new byte[ret.PartFileNameLength];

            if (stream.Read(ret.FileName, 0, ret.PartFileNameLength) != ret.PartFileNameLength)
                throw new Exception("Filename in header not long enough");

            if(ret.PartFileHashLength > 0)
            {
                ret.Hash = new byte[ret.PartFileHashLength];
                if (stream.Read(ret.Hash, 0, ret.PartFileHashLength) != ret.PartFileHashLength)
                    throw new Exception("Hash in header not long enough");
            }

            return ret;
        }

        public static Header CreateHeader(byte[] firstPart)
        {
            Header ret = new Header();
            ushort savedTotalHeaderSize = 0;

            if (firstPart.Length < FirstReadSize)
                throw new ArgumentException("firstPart too short");

            int index = 0;
            var guidArray = new byte[16];

            foreach (byte b in Encoding.ASCII.GetBytes(FilePreambule))
                if (b != firstPart[index++])
                    throw new Exception("Incorrect file preambule");

            if(BitConverter.ToUInt16(firstPart, index) != FileFormatVersion)
                throw new Exception("Incorrect file version");
            index += sizeof(ushort);

            savedTotalHeaderSize = BitConverter.ToUInt16(firstPart, index);
            index += sizeof(ushort);

            ret.PartIndex = firstPart[index++];
            ret.TotalPartCount = firstPart[index++];

            Array.Copy(firstPart, index, guidArray, 0, 16);
            ret.SplitFormat = new Guid(guidArray);
            index += 16;

            ret.OriginalFileLength = BitConverter.ToInt64(firstPart, index);
            index += sizeof(long);

            ret.PartFileNameLength = BitConverter.ToUInt16(firstPart, index);
            index += sizeof(ushort);

            ret.TotalFileNameLength = BitConverter.ToUInt16(firstPart, index);
            index += sizeof(ushort);

            Array.Copy(firstPart, index, guidArray, 0, 16);
            ret.HashFormat = new Guid(guidArray);
            index += 16;

            ret.PartFileHashLength = BitConverter.ToUInt16(firstPart, index);
            index += sizeof(ushort);

            ret.TotalFileHashLength = BitConverter.ToUInt16(firstPart, index);
            index += sizeof(ushort);

            if (savedTotalHeaderSize != ret.TotalHeaderSize)
                throw new Exception("Saved Header size is different than computed one.");

            return ret;
        }

        public byte[] WriteHeader()
        {
            byte[] ret = new byte[TotalHeaderSize];

            Encoding.ASCII.GetBytes(FilePreambule).CopyTo(ret, 0);
            BitConverter.GetBytes(FileFormatVersion).CopyTo(ret, 5);
            BitConverter.GetBytes(TotalHeaderSize).CopyTo(ret, 7);
            ret[9] = PartIndex;
            ret[10] = TotalPartCount;
            SplitFormat.ToByteArray().CopyTo(ret, 11);
            BitConverter.GetBytes(OriginalFileLength).CopyTo(ret, 27);
            BitConverter.GetBytes(PartFileNameLength).CopyTo(ret, 35);
            BitConverter.GetBytes(TotalFileNameLength).CopyTo(ret, 37);
            HashFormat.ToByteArray().CopyTo(ret, 39);
            BitConverter.GetBytes(PartFileHashLength).CopyTo(ret, 55);
            BitConverter.GetBytes(TotalFileHashLength).CopyTo(ret, 57);

            if (PartFileNameLength > 0)
                FileName.CopyTo(ret, FirstReadSize);
            int index = FirstReadSize + PartFileNameLength;

            if (PartFileHashLength > 0)
                Hash.CopyTo(ret, index);
            return ret;
        }
    }
}
