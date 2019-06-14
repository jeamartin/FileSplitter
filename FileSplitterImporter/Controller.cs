using FileSplitterCommon;
using FileSplitterDef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplitterImporter
{
    public class Controller
    {
        public void Merge(string target, string source, Type readType, Type writeType, bool restoreOriginalFileName = false)
        {
            if (!readType.GetInterfaces().Contains(typeof(IGenReader)))
                throw new ArgumentException("Incompatible type reader");
            if (!writeType.GetInterfaces().Contains(typeof(IGenWriter)))
                throw new ArgumentException("Incompatible type writer");

            //1déterminer le nombre de partie en lisant le header du fichier source
            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                var header = Header.CreateHeader(reader.Reader);
                var headers = new Header[header.TotalPartCount];

                //2vérifier la présence des fichiers sources nécessaires
                for (byte i = 0; i < header.TotalPartCount; i++)
                {
                    string fileFullPath = Utils.GetFileNameByIndex(source, i);
                    if (!File.Exists(fileFullPath)) //TODO: il faudrait être indépendant du fichier ici.
                        throw new Exception("File not found" + fileFullPath);
                }
                //3instancier les readers nécessaires
                IGenReader[] readers = new IGenReader[header.TotalPartCount];
                try
                {
                    for (byte i = 0; i < header.TotalPartCount; i++)
                    {
                        if (i == header.PartIndex)
                            readers[i] = reader;
                        else
                            readers[i] = (IGenReader)Activator.CreateInstance(readType);
                    }
                    //4 read all header
                    for (byte i = 0; i < header.TotalPartCount; i++)
                    {
                        if (i != header.PartIndex)
                        {
                            readers[i].Open(Utils.GetFileNameByIndex(source, i));
                            headers[i] = Header.CreateHeader(readers[i].Reader);
                        }
                        else
                        {
                            headers[i] = header;
                        }
                    }

                    var merger = (IFileMerger)Activator.CreateInstance(ImportedFactory.Instance.GetMergerTypeByProtocol(header.SplitFormat));
                    string origFileName = "";
                    if (restoreOriginalFileName)
                    {
                        origFileName = MergeFileName(headers, merger, header.TotalPartCount, header.TotalFileNameLength);
                        System.Diagnostics.Debug.WriteLine(origFileName);
                    }
                    //5instancier le writer nécessaire (target)
                    using (var writer = (IGenWriter)Activator.CreateInstance(writeType))
                    {
                        if (restoreOriginalFileName)
                            writer.Open(Path.GetDirectoryName(target) + Path.DirectorySeparatorChar + origFileName);
                        else
                            writer.Open(target);

                        Stream[] sources = new Stream[header.TotalPartCount];
                        for (byte i = 0; i < header.TotalPartCount; i++)
                            sources[i] = readers[i].Reader;

                        merger.Merge(writer.Writer, sources, header.TotalHeaderSize, header.TotalPartCount, header.OriginalFileLength);
                    }
                }
                finally
                {
                    for (byte i = 0; i < header.TotalPartCount; i++)
                        if (i != header.TotalPartCount && readers[i] != null)
                            readers[i].Dispose();
                }
            }
        }

        public void Split(string source, long sourceLength, string targetPath, Type readType, Type writeType, Type spliterType, byte numberOfPart)
        {
            if (!readType.GetInterfaces().Contains(typeof(IGenReader)))
                throw new ArgumentException("Incompatible type reader");
            if (!writeType.GetInterfaces().Contains(typeof(IGenWriter)))
                throw new ArgumentException("Incompatible type writer");
            if (!spliterType.GetInterfaces().Contains(typeof(IFileSpliter)))
                throw new ArgumentException("Incompatible type spliter");

            var writers = new IGenWriter[numberOfPart];
            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                try
                {
                    var spliter = (IFileSpliter)Activator.CreateInstance(spliterType);
                    var filename = Path.GetFileName(source);
                    var filenamePart = SplitFileName(filename, spliter, numberOfPart);
                    Stream[] targets = new Stream[numberOfPart];

                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        writers[i] = (IGenWriter)Activator.CreateInstance(writeType);
                        writers[i].Open(Utils.WriteFileName(source, targetPath, i));
                        targets[i] = writers[i].Writer;
                        var header = new Header()
                        {
                            PartIndex = i,
                            FileName = filenamePart[i],
                            HashFormat = Guid.Empty,
                            PartFileHashLength = 0,
                            OriginalFileLength = sourceLength,
                            PartFileNameLength = (ushort)filenamePart[i].Length,
                            SplitFormat = spliter.Protocol,
                            TotalPartCount = numberOfPart,
                            TotalFileNameLength = (ushort)filename.Length
                        };
                        targets[i].Write(header.WriteHeader(), 0, header.TotalHeaderSize);
                    }

                    spliter.Split(targets, reader.Reader, numberOfPart);
                }
                finally
                {
                    for (byte i = 0; i < numberOfPart; i++)
                        if (writers[i] != null)
                            writers[i].Dispose();
                }

            }
        }


        public void Split(string source, Type readType, Type writeType, Type spliterType, byte numberOfPart)
        {
            if (!readType.GetInterfaces().Contains(typeof(IGenReader)))
                throw new ArgumentException("Incompatible type reader");
            if (!writeType.GetInterfaces().Contains(typeof(IGenWriter)))
                throw new ArgumentException("Incompatible type writer");
            if (!spliterType.GetInterfaces().Contains(typeof(IFileSpliter)))
                throw new ArgumentException("Incompatible type spliter");

            var writers = new IGenWriter[numberOfPart];
            string targetFolder = Path.GetDirectoryName(source); //TODO: a mettre en paramètres ? 

            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                try
                {
                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        writers[i] = (IGenWriter)Activator.CreateInstance(writeType);
                        writers[i].Open(Utils.WriteFileName(source, targetFolder, i));
                    }

                    var spliter = (IFileSpliter)Activator.CreateInstance(spliterType);

                    Stream[] targets = new Stream[numberOfPart];
                    for (byte i = 0; i < numberOfPart; i++)
                        targets[i] = writers[i].Writer;

                    for (byte i = 0; i < numberOfPart; i++)
                        targets[i].Write(Utils.GetHeader(reader.Length, numberOfPart, i), 0, 10);

                    spliter.Split(targets, reader.Reader, numberOfPart);
                }
                finally
                {
                    for (byte i = 0; i < numberOfPart; i++)
                        if (writers != null)
                            writers[i].Dispose();
                }
            }
        }
        [Obsolete]
        public void Merge(string target, string source, Type readType, Type writeType, Type mergerType)//TODO : function guessing mergerType
        {
            if (!readType.GetInterfaces().Contains(typeof(IGenReader)))
                throw new ArgumentException("Incompatible type reader");
            if (!writeType.GetInterfaces().Contains(typeof(IGenWriter)))
                throw new ArgumentException("Incompatible type writer");
            if (!mergerType.GetInterfaces().Contains(typeof(IFileMerger)))
                throw new ArgumentException("Incompatible type merger");

            //1déterminer le nombre de partie en lisant le header du fichier source
            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                reader.BufferSize = 10;
                if (reader.Read(10) < 10)
                {
                    throw new Exception("bad shrd file format.");
                }
                byte[] header = new byte[reader.BufferSize];

                reader.Buffer.CopyTo(header, 0);

                byte numberOfPart = Utils.GetQtyTotal(header);
                byte indexOfSource = Utils.GetIndex(header);
                long srcLength = Utils.GetLengthFromHeader(header);

                //2vérifier la présence des fichiers sources nécessaires
                for (byte i = 0; i < numberOfPart; i++)
                {
                    string fileFullPath = Utils.GetFileNameByIndex(source, i);
                    if (!File.Exists(fileFullPath)) //TODO: il faudrait être indépendant du fichier ici.
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
                        readers[i].BufferSize = 10;//header size
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

                        var merger = (IFileMerger)Activator.CreateInstance(mergerType);

                        Stream[] sources = new Stream[numberOfPart];
                        for (byte i = 0; i < numberOfPart; i++)
                            sources[i] = readers[i].Reader;

                        merger.Merge(writer.Writer, sources, header);
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

        byte[][] SplitFileName(string filename, IFileSpliter spliter, byte numberOfPart)
        {
            var WriteStreams = new MemoryStream[numberOfPart];
            var ReadStream = new MemoryStream(Encoding.Default.GetBytes(filename));
            byte[][] ret = new byte[numberOfPart][];
            for (byte i = 0; i < numberOfPart; i++)
                WriteStreams[i] = new MemoryStream();

            spliter.Split(WriteStreams, ReadStream, numberOfPart);

            for (byte i = 0; i < numberOfPart; i++)
            {
                WriteStreams[i].Position = 0;
                byte [] buffer = new byte[WriteStreams[i].Length];
                int lng = WriteStreams[i].Read(buffer, 0, buffer.Length);

                ret[i] = new byte[lng];
                Array.Copy(buffer, 0, ret[i], 0, lng);
            }
            return ret;
        }

        string MergeFileName(Header[] headers, IFileMerger merger, byte numberOfPart, long totalLength)
        {
            var ReadStreams = new MemoryStream[numberOfPart];
            for (byte i = 0; i < numberOfPart; i++)
                ReadStreams[i] = new MemoryStream(headers[i].FileName);

            var WriteStream = new MemoryStream();

            merger.Merge(WriteStream, ReadStreams, 0, numberOfPart, totalLength);
            WriteStream.Position = 0;

            byte[] buffer = new byte[WriteStream.Length];

            int lng = WriteStream.Read(buffer, 0, buffer.Length);

            var filenameBytes = new byte[lng];

            Array.Copy(buffer, 0, filenameBytes, 0, lng);

            return Encoding.Default.GetString(filenameBytes);
        }
    }
}
