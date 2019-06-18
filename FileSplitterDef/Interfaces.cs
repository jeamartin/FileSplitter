using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplitterDef
{
    public static class FileSplitterCommon
    {
        public const string FILE_EXT = "shrd";
    }
    //using FileStream for file or webrequest for http
    public interface IGenReader : IBufferized, IDisposable
    {
        string Protocol { get; }
        Stream Reader { get; }
        long Length { get; }
        //open
        void Open(string target);
        //read chunk
        int Read(int count);
        //close
        new void Dispose();
    }
    public interface IGenWriter : IBufferized, IDisposable
    {
        string Protocol { get; }
        Stream Writer { get; }
        //open
        void Open(string target);
        //read chunk
        void Write(int count);
        //close
        new void Dispose();
    }

    public interface IBufferized
    {
        int BufferSize { get; set; }

        byte[] Buffer { get; set; }
    }

    public interface IFileMerger
    {
        string UserName { get; }
        Guid Protocol { get; }
        void Merge(Stream target, Stream[] sources, int position, byte numberOfPart, long totalLength);
    }
    public interface IFileSpliter
    {
        string UserName { get; }
        Guid Protocol { get; }
        void Split(Stream[] targets, Stream source, byte numberOfPart);
    }
}