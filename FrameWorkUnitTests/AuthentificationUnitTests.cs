using FrameWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class AuthentificationUnitTests
    {
        private readonly string masterPassword = "NotVerySecurePassword";
        private static string _currentDir = Directory.GetCurrentDirectory();

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            CommonInit.Init();
            CommonInit.LoadSettings();
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            CommonInit.CleanUp();
        }

        [TestMethod]
        public void CheckMasterPasswordTest()
        {
            SecureString wrongPassword = new SecureString();
            wrongPassword.AppendChar('c');
            SecureString passwd = new SecureString();
            foreach (char c in masterPassword.ToCharArray())
                passwd.AppendChar(c);

            Authentification.NewMasterPassword(passwd, triggerCompleteEvent:true);
            PasswordObject oldAppPassword = new PasswordObject();
            oldAppPassword.Password = Authentification.AppPassword.Password.Copy();
            oldAppPassword.Salt = new byte[Authentification.AppPassword.Salt.Length];
            Authentification.AppPassword.Salt.CopyTo(oldAppPassword.Salt, 0);

            Assert.IsFalse(Authentification.CheckMasterPassword(wrongPassword));

            Assert.IsTrue(Authentification.CheckMasterPassword(passwd));
            PasswordObject newAppPassword = new PasswordObject();
            newAppPassword.Password = Authentification.AppPassword.Password.Copy();
            newAppPassword.Salt = new byte[Authentification.AppPassword.Salt.Length];
            Authentification.AppPassword.Salt.CopyTo(newAppPassword.Salt, 0);
            Assert.IsTrue(CheckAppPassword(oldAppPassword, newAppPassword));

            Assert.IsTrue(Authentification.CheckMasterPassword(passwd, shortCheck:true));
            Assert.IsTrue(CheckAppPassword(newAppPassword, Authentification.AppPassword));
        }

        private bool CheckAppPassword(PasswordObject oldPasswd, PasswordObject newPasswd)
        {
            if (!GetSecureStringValue(oldPasswd.Password).Equals(GetSecureStringValue(newPasswd.Password)))
                return false;
            if (oldPasswd.Salt.Length != newPasswd.Salt.Length)
                return false;
            if (!oldPasswd.Salt.OrderBy(s => s).SequenceEqual(newPasswd.Salt.OrderBy(s => s)))
                return false;
            return true;
        }

        private string GetSecureStringValue(SecureString secureString)
        {
            var unsecurePointerThis = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return System.Runtime.InteropServices.Marshal.PtrToStringUni(unsecurePointerThis);
        }
    }
}
