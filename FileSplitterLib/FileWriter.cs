﻿using FileSplitterDef;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplitterLib
{
    [Export(typeof(IGenWriter))]
    public class FileWriter : IGenWriter
    {
        FileStream writer;
        int bufferSize;
        byte[] buffer;

        public int BufferSize { get => bufferSize; set { bufferSize = value; buffer = new byte[bufferSize]; } }

        public byte[] Buffer { get => buffer; set => buffer = value; }

        public string Protocol { get => "file"; }

        public Stream Writer { get => writer; }

        public void Open(string target)
        {
            if(target is string)
                writer = new FileStream(target as string, FileMode.Create, FileAccess.Write);
            else
                throw new Exception("invalid type");
        }

        public void Write(int count)
        {
            writer.Write(buffer, 0, count);
            writer.Flush();
        }

        public void Dispose()
        {
            writer.Close();
            writer.Dispose();
        }
    }
}
