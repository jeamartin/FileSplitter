using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using FileSplitterDef;

namespace FileSplitterLib
{
    [Export(typeof(IGenReader))]
    public class FileReader : IGenReader
    {
        FileStream reader;
        int bufferOffset = 0;
        int bufferSize;
        byte[] buffer;

        public int BufferSize { get => bufferSize; set { bufferSize = value; buffer = new byte[bufferSize]; } }

        public byte[] Buffer { get => buffer; set => buffer = value; }

        public string Protocol { get => "file"; }

        public Stream Reader { get => reader; }

        //open
        public void Open(string target) 
        {
            if (target is string)
                reader = new FileStream(target as string, FileMode.Open, FileAccess.Read); //File.Open(targetUri, FileMode.Open));
            else
                throw new Exception("invalid type");
        }
        //read chunk
        public int Read(int count)
        {
            return reader.Read(buffer, bufferOffset, count);
        }
        //close
        public void Dispose()
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }
    }
}
