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
                writer.Write(new byte[2] { 65, 97 }, 2);
            }
        }

        static void ReadFile()
        {
            var buffer = new byte[2];

            using (var reader = factory.GetReaderByProtocol("file"))
            {
                reader.Open("fileWrite.tst");
                reader.Read(ref buffer, 2);
            }

            Assert.IsTrue(buffer[0] == 65 && buffer[1] == 97);
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
