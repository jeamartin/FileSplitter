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

        //open
        public void Open(string targetUri) 
        {
            reader = new FileStream(targetUri, FileMode.Open, FileAccess.Read); //File.Open(targetUri, FileMode.Open));
        }
        //read chunk
        public int Read(int count)
        {
            return reader.Read(buffer, bufferOffset, count);
        }
        //close
        public void Dispose()
        {
            reader.Close();
            reader.Dispose();
        }
    }
}
