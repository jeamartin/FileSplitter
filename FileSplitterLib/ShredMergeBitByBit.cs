using FileSplitterDef;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplitterLib
{
    [Export(typeof(IFileMerger))]
    [Export(typeof(IFileSpliter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShredMergeBitByBit : IFileMerger, IFileSpliter
    {
        public string Protocol {
            get { return "ByBit"; }
        }

        static byte[] BYTE_MASK = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 };

        const byte LESS_SIG_MASK = 0x1;
        public void Merge(string target, string source, Type readType, Type writeType)
        {
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
                        byte[] curRead = new byte[numberOfPart];
                        //byte[] curReadIndex = new byte[numberOfPart];
                        byte[] writeBuffer = new byte[numberOfPart];
                        byte[] oneByteBuffer = new byte[1];
                        long totalWrited = 0;

                        while (!oneEndOfFile)
                        {
                            //6 retrouver 1 byte de toutes les parties
                            for (byte i = 0; i < numberOfPart; i++)
                            {
                                if (readers[i].Read(ref oneByteBuffer, 1) > 0)
                                    curRead[i] = oneByteBuffer[0];
                                else
                                {
                                    curRead[i] = 0;
                                    oneEndOfFile = true;
                                }
                                //curReadIndex[i] = 0;
                            }
                            //7réunir les éléments dans le buffer.
                            //if (!oneEndOfFile)
                            //{
                            int position = 0;
                            for (byte i = 0; i < 8; i++)
                                for (byte j = 0; j < numberOfPart; j++)
                                {
                                    //Console.WriteLine((i * j) / 8 + "<->" + position / 8);
                                    if ((curRead[j] & BYTE_MASK[i]) > 0)
                                        writeBuffer[position / 8] += BYTE_MASK[position % 8];
                                    position++;
                                }
                            //}
                            //8écriture du buffer
                            if (!oneEndOfFile)
                            {
                                byte qtyToWrite = totalWrited + numberOfPart > length ? (byte)(length - totalWrited) : numberOfPart;

                                writer.Write(writeBuffer, qtyToWrite);
                                totalWrited += qtyToWrite;
                            }

                            //9 réinit shorter than reassign ? 
                            for (int i = 0; i < writeBuffer.Length; i++)
                                writeBuffer[i] = 0;
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
                    long cursor = 0;
                    byte[] curWrite = new byte[numberOfPart];
                    byte[] curWriteIndex = new byte[numberOfPart];
                    initByteArray(ref curWrite);
                    initByteArray(ref curWriteIndex);

                    while ((qtyRead = reader.Read(ref curRead, 1024)) > 0)
                    {
                        //sourceLength += qtyRead;
                        for (int i = 0; i < qtyRead; i++)
                        {
                            byte cur = curRead[i];
                            for (byte j = 0; j < 8; j++)
                            {
                                byte part = (byte)(cursor % numberOfPart);
                                //1 copier le bit plus a droite dans curWrite part
                                if ((cur & LESS_SIG_MASK) > 0)
                                    curWrite[part] += (byte)Math.Pow(2, curWriteIndex[part]);
                                //2incrémenter la partie a écrire
                                curWriteIndex[part]++;
                                //3 décaler le byte cur en fonction
                                cur = (byte)(cur >> 1);

                                //4 si curwriteindex == 7 alors écrire curWrite[part] dans fsWrite[part]
                                if (curWriteIndex[part] == 8)
                                {
                                    writer[part].Write(oneByteArray(curWrite[part]), 1);
                                    //puis réinitialiser curwrite[part] et curWriteIndexDePart
                                    curWrite[part] = 0;
                                    curWriteIndex[part] = 0;
                                }
                                cursor++;
                            }
                        }
                    }
                    //5 écrire le reste (s'il y en a un) de curWrite dans writer 
                    if (sourceLength % numberOfPart > 0)
                        for (byte i = 0; i < numberOfPart; i++)
                            writer[i].Write(oneByteArray(curWrite[i]), 1);

                }
                finally
                {
                    for (byte i = 0; i < numberOfPart; i++)
                        if(writer != null)
                            writer[i].Dispose();
                }
            }
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

        string getFileNameByIndex(string source, byte index)
        {
            return source.Substring(0, source.Length - 8) + (index).ToString().PadLeft(3, '0') + "." + FileSplitterCommon.FILE_EXT;
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

        void initByteArray(ref byte[] toInit)
        {
            for (int i = 0; i < toInit.Length; i++)
                toInit[i] = 0;
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


    }
}
