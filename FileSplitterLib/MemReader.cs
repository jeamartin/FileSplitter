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
    [Export(typeof(IGenReader))]
    public class MemReader : IGenReader
    {
        MemoryStream reader;
        int bufferOffset = 0;
        int bufferSize;
        byte[] buffer;

        public int BufferSize { get => bufferSize; set { bufferSize = value; buffer = new byte[bufferSize]; } }

        public byte[] Buffer { get => buffer; set => buffer = value; }

        public string Protocol { get => "memory"; }

        public Stream Reader { get => reader; }
        //open
        public void Open(string target)
        {
           /* if(target is byte[])
                reader = new MemoryStream(target as byte[]);
            else*/
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
            reader.Close();
            reader.Dispose();
        }

    }
}
