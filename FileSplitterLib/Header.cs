using System;
using System.Collections.Generic;
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
H. Original file name length     2  bytes (sometime filename can be longer)
I. File name or part             H, if part -> significante part -> size = H / E * (F overhead)
J. Format de hash				 16 bytes (00000000-0000-0000-0000-000000000000) mean no hash (1 = tbd)
K. Original file hash length     2  bytes (0 no hash)
L. Hash or hash part             K, if part -> significante part -> size = K / E * (F overhead)
M. File part					 variable size = G / E * (F overhead)
*/


namespace FileSplitterLib
{
    public class Header
    {
        public const string FilePreambule = "split";
        public ushort FileFormatVersion = 1;

        public ushort TotalHeaderSize => ((ushort) (55 + OriginalFileNameLength + OriginalFileHashLength));

        public byte PartIndex;
        public byte TotalPartCount;
        public Guid SplitFormat = new Guid();
        public ulong OriginalFileLength;
        public ushort OriginalFileNameLength;
        public byte[] FileName;
        public Guid HashFormat = new Guid();
        public ushort OriginalFileHashLength;
        public byte[] Hash;


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
            BitConverter.GetBytes(OriginalFileNameLength).CopyTo(ret, 29);
            FileName.CopyTo(ret, 31);
            int index = 31 + OriginalFileNameLength;

            HashFormat.ToByteArray().CopyTo(ret, index);
            index += 16;
            BitConverter.GetBytes(OriginalFileHashLength).CopyTo(ret, index);
            index += sizeof(ushort);
            Hash.CopyTo(ret, index);

            return ret;
        }
    }
}
