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

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            if (IOProxy.Exists(".key_backup"))
                File.Delete(IOProxy.FileNameToCode(".key_backup"));
            if (IOProxy.Exists(".bak_key_backup"))
                File.Delete(IOProxy.FileNameToCode(".bak_key_backup"));

            if (Application.Current == null)
            { new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; }

            string path = Directory.GetCurrentDirectory();
            var pathArray = path.Split(new string[] { "FrameWorkUnitTests" }, StringSplitOptions.None);
            Directory.SetCurrentDirectory(pathArray[0] + "FrameWork" + pathArray[1]);

            Application.ResourceAssembly = System.Reflection.Assembly.GetAssembly(typeof(MainWindow));
            var app = new MainWindow();
            app.InitializeComponent();

            Settings.LoadSettings();

            if (IOProxy.Exists(".key"))
                File.Move(IOProxy.FileNameToCode(".key"), IOProxy.FileNameToCode(".key_backup"));
            if (IOProxy.Exists(".bak_key"))
                File.Move(IOProxy.FileNameToCode(".bak_key"), IOProxy.FileNameToCode(".bak_key_backup"));
        }

        [TestMethod]
        public void CheckMasterPasswordTest()
        {
            SecureString wrongPassword = new SecureString();
            wrongPassword.AppendChar('c');
            SecureString passwd = new SecureString();
            foreach (char c in masterPassword.ToCharArray())
                passwd.AppendChar(c);

            Authentification.NewMasterPassword(passwd);
            PasswordObject oldAppPassword =  Authentification.AppPassword;
            File.Delete(IOProxy.FileNameToCode(".bak_key"));

            Assert.IsFalse(Authentification.CheckMasterPassword(wrongPassword));

            Assert.IsTrue(Authentification.CheckMasterPassword(passwd));
            PasswordObject newAppPassword = Authentification.AppPassword;
            Assert.IsTrue(CheckAppPassword(oldAppPassword, newAppPassword));

            Assert.IsTrue(Authentification.CheckMasterPassword(passwd));
            Assert.IsTrue(CheckAppPassword(newAppPassword, Authentification.AppPassword));

            File.Delete(IOProxy.FileNameToCode(".bak_key"));
            Assert.IsTrue(Authentification.CheckMasterPassword(passwd, true));
            Assert.IsTrue(CheckAppPassword(newAppPassword, Authentification.AppPassword));
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            if (IOProxy.Exists(".key"))
                File.Delete(IOProxy.FileNameToCode(".key"));
            if (IOProxy.Exists(".bak_key"))
                File.Delete(IOProxy.FileNameToCode(".bak_key"));

            File.Move(IOProxy.FileNameToCode(".key_backup"), IOProxy.FileNameToCode(".key"));
            File.Move(IOProxy.FileNameToCode(".bak_key_backup"), IOProxy.FileNameToCode(".bak_key"));
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
