using FileSplitterCommon;
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
        public string UserName
        {
            get { return "ByBit"; }
        }

        public Guid Protocol
        {
            get { return new Guid("0A8228B2-2F3F-4340-A6F3-91E070BAACEC"); }
        }

        static byte[] BYTE_MASK = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 };

        const byte LESS_SIG_MASK = 0x1;

        public void Merge(Stream target, Stream[] sources, int position, byte numberOfPart, long totalLength)
        {

            if (sources.Length < 2)
                throw new ArgumentException("You need at least 2 sources");

            //ensure all the source stream positions are after the header.
            foreach (var source in sources)
                if (source.Position < position)
                    source.Position = position;


            /*byte numberOfPart = Utils.GetQtyTotal(header);
            byte indexOfSource = Utils.GetIndex(header);
            long length = Utils.GetLengthFromHeader(header);*/

            bool oneEndOfFile = false;
            byte[] oneByteBuffer = new byte[1];
            byte[] curRead = new byte[numberOfPart];
            byte[] writeBuffer = new byte[numberOfPart];
            long totalWrited = 0;

            while (!oneEndOfFile)
            {
                //6 retrouver 1 byte de toutes les parties
                for (byte i = 0; i < numberOfPart; i++)
                {
                    if (sources[i].Read(oneByteBuffer, 0, 1) < 1)
                    {
                        curRead[i] = 0;
                        oneEndOfFile = true;
                    }
                    else
                        curRead[i] = oneByteBuffer[0];
                }
                //7réunir les éléments dans le buffer.
                int pos = 0;
                for (byte i = 0; i < 8; i++)
                    for (byte j = 0; j < numberOfPart; j++)
                    {
                        //Console.WriteLine((i * j) / 8 + "<->" + position / 8);
                        if ((curRead[j] & BYTE_MASK[i]) > 0)
                            writeBuffer[pos / 8] += BYTE_MASK[pos % 8];
                        pos++;
                    }
                //8écriture du buffer
                if (!oneEndOfFile)
                {
                    byte qtyToWrite = totalWrited + numberOfPart > totalLength ? (byte)(totalLength - totalWrited) : numberOfPart;

                    target.Write(writeBuffer, 0, qtyToWrite);
                    totalWrited += qtyToWrite;
                }

                //9 réinit shorter than reassign ? 
                for (int i = 0; i < writeBuffer.Length; i++)
                    writeBuffer[i] = 0;
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
            long length = Utils.GetLengthFromHeader(header);

            bool oneEndOfFile = false;
            byte[] oneByteBuffer = new byte[1];
            byte[] curRead = new byte[numberOfPart];
            byte[] writeBuffer = new byte[numberOfPart];
            long totalWrited = 0;

            while (!oneEndOfFile)
            {
                //6 retrouver 1 byte de toutes les parties
                for (byte i = 0; i < numberOfPart; i++)
                {
                    if (sources[i].Read(oneByteBuffer, 0, 1) < 1)
                    {
                        curRead[i] = 0;
                        oneEndOfFile = true;
                    }
                    else
                        curRead[i] = oneByteBuffer[0];
                }
                //7réunir les éléments dans le buffer.
                int position = 0;
                for (byte i = 0; i < 8; i++)
                    for (byte j = 0; j < numberOfPart; j++)
                    {
                        //Console.WriteLine((i * j) / 8 + "<->" + position / 8);
                        if ((curRead[j] & BYTE_MASK[i]) > 0)
                            writeBuffer[position / 8] += BYTE_MASK[position % 8];
                        position++;
                    }
                //8écriture du buffer
                if (!oneEndOfFile)
                {
                    byte qtyToWrite = totalWrited + numberOfPart > length ? (byte)(length - totalWrited) : numberOfPart;

                    target.Write(writeBuffer, 0, qtyToWrite);
                    totalWrited += qtyToWrite;
                }

                //9 réinit shorter than reassign ? 
                for (int i = 0; i < writeBuffer.Length; i++)
                    writeBuffer[i] = 0;
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
                long length = Utils.GetLengthFromHeader(reader.Buffer);

                //2vérifier la présence des fichiers sources nécessaires
                for (byte i = 0; i < numberOfPart; i++)
                {
                    string fileFullPath = Utils.GetFileNameByIndex(source, i);
                    if (!File.Exists(fileFullPath))
                        throw new Exception("File not found " + fileFullPath);
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
                    {
                        if (i != indexOfSource)
                        {
                            readers[i].Open(Utils.GetFileNameByIndex(source, i));
                            readers[i].BufferSize = 10;
                            readers[i].Read(10);
                        }
                        readers[i].BufferSize = 1;
                    }
                    //5instancier le writer nécessaire (target)
                    using (var writer = (IGenWriter)Activator.CreateInstance(writeType))
                    {
                        writer.Open(target);
                        writer.BufferSize = numberOfPart;
                        bool oneEndOfFile = false;
                        byte[] oneByteBuffer = new byte[1];
                        long totalWrited = 0;

                        while (!oneEndOfFile)
                        {
                            //6 retrouver 1 byte de toutes les parties
                            for (byte i = 0; i < numberOfPart; i++)
                            {
                                if (readers[i].Read(1) < 1)
                                { 
                                    readers[i].Buffer[0] = 0;
                                    oneEndOfFile = true;
                                }
                            }
                            //7réunir les éléments dans le buffer.
                            int position = 0;
                            for (byte i = 0; i < 8; i++)
                                for (byte j = 0; j < numberOfPart; j++)
                                {
                                    //Console.WriteLine((i * j) / 8 + "<->" + position / 8);
                                    if ((readers[j].Buffer[0] & BYTE_MASK[i]) > 0)
                                        writer.Buffer[position / 8] += BYTE_MASK[position % 8];
                                    position++;
                                }
                            //8écriture du buffer
                            if (!oneEndOfFile)
                            {
                                byte qtyToWrite = totalWrited + numberOfPart > length ? (byte)(length - totalWrited) : numberOfPart;

                                writer.Write(qtyToWrite);
                                totalWrited += qtyToWrite;
                            }

                            //9 réinit shorter than reassign ? 
                            for (int i = 0; i < writer.Buffer.Length; i++)
                                writer.Buffer[i] = 0;
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

        public void Split(Stream[] targets, Stream source, byte numberOfPart)
        {
            const int READ_SIZE = 1024;
            byte[] curRead = new byte[READ_SIZE];

            int qtyRead = 0;
            long cursor = 0;
            byte[] curWrite = new byte[numberOfPart];
            byte[] curWriteIndex = new byte[numberOfPart];
            Utils.InitByteArray(ref curWrite);
            Utils.InitByteArray(ref curWriteIndex);
            long sourceLength = 0;

            while ((qtyRead = source.Read(curRead, 0, READ_SIZE)) > 0)
            {
                sourceLength += qtyRead;
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

                        //4 si curwriteindex == 7 alors écrire writer[part].Buffer dans writer[part]
                        if (curWriteIndex[part] == 8)
                        {
                            targets[part].Write(Utils.OneByteArray(curWrite[part]), 0, 1);
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
                    targets[i].Write(Utils.OneByteArray(curWrite[i]), 0, 1);

        }

        public void Shred(string source, Type readType, Type writeType, byte numberOfPart)
        {
            if (!readType.GetInterfaces().Contains(typeof(IGenReader)))
                throw new Exception("Incompatible type reader");
            if (!writeType.GetInterfaces().Contains(typeof(IGenWriter)))
                throw new Exception("Incompatible type writer");

            var writer = new IGenWriter[numberOfPart];
            string targetFolder = Path.GetDirectoryName(source);
            long sourceLength = (new FileInfo(source)).Length; //TODO: a déplacer dans une méthode non couplée (ExtFile)
            const int READ_SIZE = 1024;

            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                reader.BufferSize = READ_SIZE;
                try
                {
                    //écriture des metainfos en header.
                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        writer[i] = (IGenWriter)Activator.CreateInstance(writeType);
                        writer[i].Open(Utils.WriteFileName(source, targetFolder, i));
                        writer[i].BufferSize = 10;
                        writer[i].Buffer = Utils.GetHeader(sourceLength, numberOfPart, i); 
                        writer[i].Write(10);
                        writer[i].BufferSize = 1;
                        writer[i].Buffer[0] = 0;
                    }

                    int qtyRead = 0;
                    long cursor = 0;
                    byte[] curWriteIndex = new byte[numberOfPart];
                    
                    Utils.InitByteArray(ref curWriteIndex);

                    while ((qtyRead = reader.Read(READ_SIZE)) > 0)
                    {
                        //sourceLength += qtyRead;
                        for (int i = 0; i < qtyRead; i++)
                        {
                            byte cur = reader.Buffer[i];
                            for (byte j = 0; j < 8; j++)
                            {
                                byte part = (byte)(cursor % numberOfPart);
                                //1 copier le bit plus a droite dans curWrite part
                                if ((cur & LESS_SIG_MASK) > 0)
                                    writer[part].Buffer[0] += (byte)Math.Pow(2, curWriteIndex[part]);
                                //2incrémenter la partie a écrire
                                curWriteIndex[part]++;
                                //3 décaler le byte cur en fonction
                                cur = (byte)(cur >> 1);

                                //4 si curwriteindex == 7 alors écrire writer[part].Buffer dans writer[part]
                                if (curWriteIndex[part] == 8)
                                {
                                    writer[part].Write(1);
                                    //puis réinitialiser curwrite[part] et curWriteIndexDePart
                                    writer[part].Buffer[0] = 0;
                                    curWriteIndex[part] = 0;
                                }
                                cursor++;
                            }
                        }
                    }
                    //5 écrire le reste (s'il y en a un) de curWrite dans writer 
                    if (sourceLength % numberOfPart > 0)
                        for (byte i = 0; i < numberOfPart; i++)
                            writer[i].Write(1);

                }
                finally
                {
                    for (byte i = 0; i < numberOfPart; i++)
                        if(writer != null)
                            writer[i].Dispose();
                }
            }
        }


    }
}
