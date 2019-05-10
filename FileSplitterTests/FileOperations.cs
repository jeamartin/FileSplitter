using FileSplitterImporter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplitterTests
{
    [TestClass()]
    public class FileOperations
    {
        static ImportedFactory factory;

        [ClassInitialize()]
        public static void Initialize(TestContext context)
        {
            factory = ImportedFactory.Instance;
            factory.DoImport();
        }

        [TestMethod]
        public void WriteAndReadBack()
        {
            WriteFile();
            ReadFile();
        }

        static void WriteFile()
        {
            using (var writer = factory.GetWriterByProtocol("file"))
            {
                writer.Open("fileWrite.tst");
                writer.BufferSize = 2;
                writer.Buffer = new byte[2] { 65, 97 };
                writer.Write(2);
            }
        }

        static void ReadFile()
        {
            using (var reader = factory.GetReaderByProtocol("file"))
            {
                reader.Open("fileWrite.tst");
                reader.BufferSize = 2;
                reader.Read(2);
                Assert.IsTrue(reader.Buffer[0] == 65 && reader.Buffer[1] == 97);
            }
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            try
            {
                System.IO.File.Delete("fileWrite.tst");
            }
            catch
            {
                //if the delete is not possible I don't care.
            }
        }

    }
}
