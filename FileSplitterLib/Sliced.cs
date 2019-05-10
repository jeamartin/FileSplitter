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

    class Sliced : IFileSpliter, IFileMerger
    {
        public string UserName
        {
            get { return "Sliced"; }
        }

        public Guid Protocol
        {
            get { return new Guid("2A63D590-46FC-4054-A3BF-BC9433267172"); }
        }

        //public void Merge(/*collection de tableau de byte en lecture*/, /*tableau d'indices de lecture*/,   /*ref tableau de bye en écriture*/, /*indice d'écriture*/) 

        public void Merge(string target, string source, Type readType, Type writeType)
        {

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

            long sliceSize = (long)Math.Ceiling(sourceLength * 1.0 / numberOfPart);

            //Trivial implementation ahead (bad for big files)
            byte[] bufRead = new byte[sliceSize];

            using (var reader = (IGenReader)Activator.CreateInstance(readType))
            {
                reader.Open(source);
                try
                {
                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        writer[i] = (IGenWriter)Activator.CreateInstance(writeType);
                        writer[i].Open(Reusables.WriteFileName(source, targetFolder, i));
                    }

                    Header h = new Header()
                    {
                        PartIndex = 0,
                        TotalPartCount = numberOfPart,
                        SplitFormat = Protocol,
                        OriginalFileLength = (ulong)sourceLength,
                        FileName = Encoding.Unicode.GetBytes(Path.GetFileName(source)),
                        OriginalFileNameLength = (ushort) Encoding.Unicode.GetByteCount(Path.GetFileName(source)),
                    };

                    for (byte i = 0; i < numberOfPart; i++)
                    {
                        h.PartIndex = i;
                        //writer[i].Write(h.WriteHeader(), h.TotalHeaderSize);

                        //var readed = reader.Read(ref bufRead, bufRead.Length);
                        //writer[i].Write(bufRead, readed);
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
    }
}
