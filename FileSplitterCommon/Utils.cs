using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplitterCommon
{
    public static class Utils
    {
        public const string FILE_EXT = "shrd";

        public static byte[] OneByteArray(byte b)
        {
            byte[] ret = new byte[1];
            ret[0] = b;
            return ret;
        }

        public static string WriteFileName(string source, string targetFolder, byte index)
        {
            string ret = (targetFolder.LastIndexOf(@"\") == targetFolder.Length - 1 ? targetFolder : targetFolder + Path.DirectorySeparatorChar) +
                Path.GetFileName(source) + "." + (index).ToString().PadLeft(3, '0') + "." + Utils.FILE_EXT;
            return ret;
        }

        public static string GetFileNameByIndex(string source, byte index)
        {
            return source.Substring(0, source.Length - 8) + (index).ToString().PadLeft(3, '0') + "." + Utils.FILE_EXT;
        }

        public static long GetLengthFromHeader(byte[] header)
        {
            return BitConverter.ToInt64(header, 0);
        }
        public static byte GetIndex(byte[] header)
        {
            return header[8];
        }

        public static byte GetQtyTotal(byte[] header)
        {
            return header[9];
        }

        public static byte[] GetHeader(long sourceLength, byte numberOfPart, byte index)
        {
            byte[] length = BitConverter.GetBytes(sourceLength);

            byte[] ret = new byte[10];

            length.CopyTo(ret, 0);
            ret[8] = index;
            ret[9] = numberOfPart;
            return ret;
        }

        public static void InitByteArray(ref byte[] toInit)
        {
            for (int i = 0; i < toInit.Length; i++)
                toInit[i] = 0;
        }
    }
}
