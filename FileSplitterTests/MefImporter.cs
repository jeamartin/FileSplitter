using System;
using FileSplitterImporter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileSplitterTests
{
    [TestClass()]
    public class MefImporter
    {
        static ImportedFactory factory;

        [ClassInitialize()]
        public static void Initialize(TestContext context)
        {
            factory = ImportedFactory.Instance;
            factory.DoImport();
        }

        [TestMethod]
        public void AtLeastOneProtocolLoaded()
        {
            Assert.IsTrue(factory.AvailableNumberOfProtocol > 0); 
        }

        [TestMethod]
        public void FileProtocolLoaded()
        {
            Assert.IsTrue(factory.ProtocolExist("file"));
        }

        [TestMethod]
        public void HttpProtocolLoaded()
        {
            Assert.IsTrue(factory.ProtocolExist("http"));
        }
    }
}
