using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplitterTests
{
    [TestClass()]
    public sealed class EventOrder
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            Debug.Print("AssemblyInit " + context.TestName);
        }

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            Debug.Print("ClassInit " + context.TestName);
        }

        [TestInitialize()]
        public void Initialize()
        {
            Debug.Print("TestMethodInit");
        }

        [TestCleanup()]
        public void Cleanup()
        {
            Debug.Print("TestMethodCleanup");
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            Debug.Print("ClassCleanup");
        }

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            Debug.Print("AssemblyCleanup");
        }

        [TestMethod()]
        [ExpectedException(typeof(System.DivideByZeroException))]
        public void DivideMethodTest()
        {
            var y = 1;
            y--;
            var i = 1 / y;
        }
    }
}
