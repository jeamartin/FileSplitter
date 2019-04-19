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
    public interface IGenReader : IDisposable
    {
        string Protocol { get; }
        //open
        void Open(string targetUri);
        //read chunk
        int Read(ref byte[] buffer, int count);
        //cloes
        new void Dispose();
    }
    public interface IGenWriter : IDisposable
    {
        string Protocol { get; }
        //open
        void Open(string targetUri);
        //read chunk
        void Write(byte[] buffer, int count);
        //close
        new void Dispose();
    }

    public interface IFileMerger
    {
        string Protocol { get; }
        void Merge(string target, string source, Type readType, Type writeType);
    }
    public interface IFileSpliter
    {
        string Protocol { get; }
        void Shred(string source, Type readType, Type writeType, byte numberOfPart);
    }
}