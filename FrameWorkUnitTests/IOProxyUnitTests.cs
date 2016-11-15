using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class IOProxyUnitTests
    {
        [TestMethod]
        public void ReadWriteMemoryStreamToFileTest()
        {

        }

        [TestMethod]
        public Task ReadWriteMemoryStreamToFileAsyncTest()
        {
            return Task.Delay(5);
        }

        [TestMethod]
        public Task WriteByteArrayToFileAsyncTest()
        {
            return Task.Delay(5);
        }

        [TestMethod]
        public void WritePasswordTest()
        {

        }

        [TestMethod]
        public void RelativeToAbsoluteTest()
        {

        }

        [TestMethod]
        public void FileNameToCodeTest()
        {

        }
    }
}
