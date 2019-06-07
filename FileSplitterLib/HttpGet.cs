using FileSplitterDef;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace FileSplitterLib
{
    [Export(typeof(IGenReader))]
    public class HttpGet : IGenReader
    {
        MemoryStream stream;
        int bufferSize;
        byte[] buffer;

        public int BufferSize { get => bufferSize; set { bufferSize = value; buffer = new byte[bufferSize]; } }

        public byte[] Buffer { get => buffer; set => buffer = value; }

        public string Protocol { get => "http"; }

        public Stream Reader { get => stream; }

        public void Open(string target)
        {
            if (target is string)
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage response = client.GetAsync(target as string).Result;

                stream = new MemoryStream(response.Content.ReadAsByteArrayAsync().Result);
            }
            else
                throw new Exception("invalid type");
        }

        public int Read(int count)
        {
            return stream.Read(buffer, 0, count);
        }

        public void Dispose()
        {
        }
    }
}
