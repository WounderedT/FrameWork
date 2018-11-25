using FrameWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class IOProxyUnitTests
    {
        private static byte[] testArray = new byte[50];
        private static MemoryStream testStream = new MemoryStream();
        private static string testFileName = "testFile";

        private static string _currentDir = Directory.GetCurrentDirectory();

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            CommonInit.Init();
            CommonInit.LoadSettings();

            Random rand = new Random();
            rand.NextBytes(testArray);
            testStream.Write(testArray, 0, testArray.Length);
            testStream.Position = 0;
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            CommonInit.CleanUp();
        }

        [TestMethod()]
        public void ReadWriteMemoryStreamToFileTest()
        {
            IOProxy.WriteMemoryStreamToFile(testStream, testFileName);
            MemoryStream ms = IOProxy.GetMemoryStreamFromFile(testFileName);
            Assert.IsTrue(CompareStreams(testStream, ms));
        }

        [TestMethod()]
        public async Task ReadWriteMemoryStreamToFileAsyncTest()
        {
            await IOProxy.WriteMemoryStreamToFileAsync(testStream, testFileName);
            MemoryStream ms = await IOProxy.GetMemoryStreamFromFileAsync(testFileName);
            Assert.IsTrue(CompareStreams(testStream, ms));
        }

        [TestMethod]
        public async Task WriteByteArrayToFileAsyncTest()
        {
            await IOProxy.WriteBytesToFileAsync(testArray, testFileName).ConfigureAwait(false);
            MemoryStream ms = await IOProxy.GetMemoryStreamFromFileAsync(testFileName).ConfigureAwait(false);
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
        public void FileNameToCodeTest()
        {
            Assert.AreEqual(IOProxy.FileNameToCode(testFileName), IOProxy.FileNameToCode(testFileName));
            Assert.AreEqual(string.Empty, IOProxy.FileNameToCode(string.Empty));
            try
            {
                IOProxy.FileNameToCode(null);
            } catch(Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NullReferenceException));
            }
            
        }

        private bool CompareStreams(MemoryStream stream1, MemoryStream stream2)
        {
            if (stream1.Length != stream2.Length)
                return false;
            while (stream1.Position < stream1.Length)
            {
                if (stream1.ReadByte() != stream2.ReadByte())
                    return false;
            }
            return true;
        }
    }
}
