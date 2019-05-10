using System;
using System.Collections.Generic;
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
        //open
        void Open(string targetUri);
        //read chunk
        int Read(int count);
        //cloes
        new void Dispose();
    }
    public interface IGenWriter : IBufferized, IDisposable
    {
        string Protocol { get; }
        //open
        void Open(string targetUri);
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
        void Merge(string target, string source, Type readType, Type writeType);
    }
    public interface IFileSpliter
    {
        string UserName { get; }
        Guid Protocol { get; }
        void Shred(string source, Type readType, Type writeType, byte numberOfPart);
    }
}