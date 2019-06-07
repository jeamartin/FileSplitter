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
        public void Merge(string target, string source, Type readType, Type writeType, Type mergerType)
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
                byte[] header = new byte[reader.BufferSize];

                reader.Buffer.CopyTo(header, 0);

                byte numberOfPart = Reusables.GetQtyTotal(header);
                byte indexOfSource = Reusables.GetIndex(header);
                long srcLength = Reusables.GetLengthFromHeader(header);

                //2vérifier la présence des fichiers sources nécessaires
                for (byte i = 0; i < numberOfPart; i++)
                {
                    string fileFullPath = Reusables.GetFileNameByIndex(source, i);
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
                            readers[i].Open(Reusables.GetFileNameByIndex(source, i));
                            readers[i].Read(10);
                        }
                    }
                    //5instancier le writer nécessaire (target)
                    using (var writer = (IGenWriter)Activator.CreateInstance(writeType))
                    {
                        writer.Open(target);

                        var merger = (IFileMerger)Activator.CreateInstance(mergerType);

                        Stream[] sources = new Stream[numberOfPart];
                        for(byte i = 0; i < numberOfPart; i++)
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
        public void Split(string source, Type readType, Type writeType, byte numberOfPart)
        {

        }
    }
}
