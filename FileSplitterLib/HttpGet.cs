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
    public class HttpGet: IGenReader
    {
        MemoryStream stream;
        int bufferSize;
        byte[] buffer;

        public int BufferSize { get => bufferSize; set { bufferSize = value; buffer = new byte[bufferSize]; } }

        public byte[] Buffer { get => buffer; set => buffer = value; }

        public string Protocol { get => "http"; }

        public void Open(string targetUri)
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.GetAsync(targetUri).Result;
            
            stream = new MemoryStream(response.Content.ReadAsByteArrayAsync().Result);
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
