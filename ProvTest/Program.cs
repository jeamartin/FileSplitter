using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSplitterImporter;

namespace ProvTest
{
    class Program
    {
        /// <summary>
        /// This was the old test project use the new test projet : FileSplitterTests
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ImportedFactory t = ImportedFactory.Instance;
            t.DoImport();
            Console.WriteLine("{0} component(s) are imported successfully.", t.AvailableNumberOfProtocol);

            //view protocol
            var result = t.EnumerateAllComponents();
            foreach (string s in result)
            {
                Console.WriteLine(s);
            }

            //test read
            if (t.ProtocolExist("file")) 
                using (var reader = t.GetReaderByProtocol("file"))
                {
                    //var buffer = new byte[1];
                    reader.Open(@"C:\tmp\test.png");
                    reader.BufferSize = 1;
                    for (int i = 0; i < 5; i++) {
                        reader.Read(1);
                        Console.Write(reader.Buffer[0]);
                    }
                }

            //test write
            if (t.ProtocolExist("file"))
                using (var writer = t.GetWriterByProtocol("file"))
                {
                    writer.Open(@"C:\tmp\writer_test.bin");
                    writer.BufferSize = 2;
                    writer.Buffer = new byte[2] { 65, 97 };
                    writer.Write(2);
                }
            /*
            var sh = t.GetSpliterByProtocol(t.GetSpliterIdByName("Sliced"));
            sh.Shred(@"C:\tmp\shred_test.bin", t.GetReaderTypeByProtocol("file"), t.GetWriterTypeByProtocol("file"), 2);

            //test shred
            var sh = t.GetSpliterByProtocol(t.GetSpliterIdByName("ByBit")) ; 
            sh.Shred(@"C:\tmp\shred_test.bin", t.GetReaderTypeByProtocol("file"), t.GetWriterTypeByProtocol("file"), 9);

            //test Merge
            var mg = t.GetMergerByProtocol(t.GetMergerIdByName("ByBit"));
            mg.Merge(@"C:\tmp\shred_test_copy.bin", @"C:\tmp\shred_test.bin.shrd001", t.GetReaderTypeByProtocol("file"), t.GetWriterTypeByProtocol("file"));

            sh = t.GetSpliterByProtocol(t.GetSpliterIdByName("Shamirs"));
            sh.Shred(@"C:\tmp\setup.exe", t.GetReaderTypeByProtocol("file"), t.GetWriterTypeByProtocol("file"), 6);

            //test recover shamir's
            mg = t.GetMergerByProtocol(t.GetMergerIdByName("Shamirs"));
            mg.Merge(@"C:\tmp\setup_shamir.exe", @"C:\tmp\setup.exe.shasec005", t.GetReaderTypeByProtocol("file"), t.GetWriterTypeByProtocol("file"));

            //test shamir's
            sh = t.GetSpliterByProtocol(t.GetSpliterIdByName("Shamirs"));
            sh.Shred(@"C:\tmp\allbytes.bin", t.GetReaderTypeByProtocol("file"), t.GetWriterTypeByProtocol("file"), 6);

            //test recover shamir's
            mg = t.GetMergerByProtocol(t.GetMergerIdByName("Shamirs"));
            mg.Merge(@"C:\tmp\allbytes_shamir.bin", @"C:\tmp\allbytes.bin.shasec005", t.GetReaderTypeByProtocol("file"), t.GetWriterTypeByProtocol("file"));

            if (t.ProtocolExist("http"))
                using (var reader = t.GetReaderByProtocol("http"))
                {
                    var buffer = new byte[150];
                    reader.Open(@"https://www.google.ch");
                    reader.Read(ref buffer, buffer.Length);
                    Console.Write(System.Text.Encoding.Default.GetString(buffer));
                }
            
            
            */
            Console.Read();
        }
    }
}
