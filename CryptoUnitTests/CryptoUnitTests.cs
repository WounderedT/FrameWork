using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace CryptoUnitTests
{
    [TestClass]
    public class CryptoUnitTests
    {
        private static char[] _masterPasswordArray = "NewSecurePassword".ToCharArray();
        private static char[] _appPasswordArray = "AnotherSecurePassword".ToCharArray();

        private static string _data = "Just some test data";
        private static byte[] _passwordSalt = new byte[] { 210, 10, 71, 160, 101, 206, 147, 14, 88,
            198, 96, 207, 208, 113, 185, 71, 179, 64, 215, 49, 238, 179, 4, 7};
        private static int _passwordIterations = 15100;

        [TestMethod]
        public void AppPasswordEncryptDecryptTest()
        {
            SecureString masterPassword, appPassword;
            GetPasswords(out masterPassword, out appPassword);
            Crypto.Crypto crypto = new Crypto.Crypto();

            var result = crypto.EncryptAppPassword(appPassword, masterPassword);
            SecureString decryptedPassord = crypto.DecryptAppPassword(result.Item1, _appPasswordArray.Length, masterPassword, result.Item2);
            Assert.AreEqual(GetSecureStringValue(appPassword), GetSecureStringValue(decryptedPassord));
        }

        [TestMethod]
        public async Task EncryptDecryptDataArrayTest()
        {
            byte[] bytes = new byte[_data.Length * sizeof(char)];
            Buffer.BlockCopy(_data.ToCharArray(), 0, bytes, 0, bytes.Length);
            SecureString masterPassword, appPassword;
            GetPasswords(out masterPassword, out appPassword);

            Crypto.Crypto crypto = new Crypto.Crypto();
            var appPasswordData = crypto.EncryptAppPassword(appPassword, masterPassword);
            byte[] encryptedResult = await crypto.EncryptDataArrayAsync(bytes, appPassword, appPasswordData.Item2);

            MemoryStream ms = new MemoryStream();
            ms.Write(encryptedResult, 0, encryptedResult.Length);
            ms.Position = 0;
            byte[] decryptedResult = crypto.DecryptMemoryStream(ms, appPassword, appPasswordData.Item2);
            Assert.IsTrue(bytes.Length == decryptedResult.Length && bytes.OrderBy(s => s).SequenceEqual(decryptedResult.OrderBy(s => s)));

            decryptedResult = await crypto.DecryptMemoryStreamAsync(ms, appPassword, appPasswordData.Item2);
            Assert.IsTrue(bytes.Length == decryptedResult.Length && bytes.OrderBy(s => s).SequenceEqual(decryptedResult.OrderBy(s => s)));
        }

        [TestMethod]
        public void GenerateSaltTest()
        {
            Crypto.Crypto crypto = new Crypto.Crypto();
            Assert.AreEqual(crypto.GenerateIterationsFromSalt(_passwordSalt), _passwordIterations);
        }

        unsafe private void GetPasswords(out SecureString masterPassword, out SecureString appPassword)
        {
            fixed (char* chars = _masterPasswordArray)
                masterPassword = new SecureString(chars, _masterPasswordArray.Length);
            fixed (char* chars = _appPasswordArray)
                appPassword = new SecureString(chars, _appPasswordArray.Length);
        }

        private string GetSecureStringValue(SecureString secureString)
        {
            var unsecurePointerThis = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return System.Runtime.InteropServices.Marshal.PtrToStringUni(unsecurePointerThis);
        }
    }
}
