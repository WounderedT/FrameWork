using FrameWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class IOProxyUnitTests
    {
        private static byte[] testArray = new byte[50];
        private static MemoryStream testStream = new MemoryStream();
        private static string testFileName = "testFile";

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Random rand = new Random();
            rand.NextBytes(testArray);
            testStream.Write(testArray, 0, testArray.Length);
            testStream.Position = 0;
        }

        [TestMethod]
        public void ReadWriteMemoryStreamToFileTest()
        {
            IOProxy.WriteMemoryStreamToFile(testStream, testFileName);
            MemoryStream ms = IOProxy.GetMemoryStreamFromFile(testFileName);
            Assert.IsTrue(CompareStreams(testStream, ms));
        }

        [TestMethod]
        public async Task ReadWriteMemoryStreamToFileAsyncTest()
        {
            await IOProxy.WriteMemoryStreamToFileAsync(testStream, testFileName);
            MemoryStream ms = await IOProxy.GetMemoryStreamFromFileAsync(testFileName);
            Assert.IsTrue(CompareStreams(testStream, ms));
        }

        [TestMethod]
        public async Task WriteByteArrayToFileAsyncTest()
        {
            await IOProxy.WriteBytesToFileAsync(testArray, testFileName);
            MemoryStream ms = await IOProxy.GetMemoryStreamFromFileAsync(testFileName);
            Assert.IsTrue(CompareStreams(testStream, ms));
        }

        [TestMethod]
        public void WritePasswordTest()
        {
            EncryptedPassword password = new EncryptedPassword(new Tuple<byte[], byte[]>(testArray, testArray));
            IOProxy.WritePassword(password, testFileName);
            EncryptedPassword newPassword = new EncryptedPassword();
            newPassword.GetPasswordFromFile(testFileName);
            Assert.IsTrue(password.Hash.OrderBy(s => s).SequenceEqual(newPassword.Hash.OrderBy(s => s)));
            Assert.IsTrue(password.Salt.OrderBy(s => s).SequenceEqual(newPassword.Salt.OrderBy(s => s)));
        }

        [TestMethod]
        public void RelativeToAbsoluteTest()
        {

        }

        [TestMethod]
        public void FileNameToCodeTest()
        {

        }

        private bool CompareStreams(MemoryStream stream1, MemoryStream stream2)
        {
            if (stream1.Length != stream2.Length)
                return false;
            while (stream1.Position <= stream1.Length)
            {
                if (stream1.ReadByte() != stream2.ReadByte())
                    return false;
            }
            return true;
        }
    }
}
