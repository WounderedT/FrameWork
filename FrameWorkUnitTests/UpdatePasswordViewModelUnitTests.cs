using FrameWork;
using FrameWork.DataModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class UpdatePasswordViewModelUnitTests
    {
        private readonly string validMasterPassword = "validMasterPassword";
        private readonly string newPassword = "newPassword";
        private readonly string wrongPassword1 = "wrongPassword";
        private readonly string wrongPassword2 = "anotherWrongPasswor";

        private FrameWork.ViewModel.UpdatePasswordViewModel viewModel = new FrameWork.ViewModel.UpdatePasswordViewModel();
        private static string _currentDir = Directory.GetCurrentDirectory();

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            if (Application.Current == null)
            { new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; }

            if (_currentDir.Contains("\\FrameWorkUnitTests\\"))
            {
                var pathArray = _currentDir.Split(new string[] { "FrameWorkUnitTests" }, StringSplitOptions.None);
                Directory.SetCurrentDirectory(pathArray[0] + "FrameWork" + pathArray[1]);
            }

            Application.ResourceAssembly = System.Reflection.Assembly.GetAssembly(typeof(MainWindow));
            Settings.LoadSettings();

            if (File.Exists(".key_backup"))
                File.Delete(".key_backup");
            if (File.Exists(".bak_key_backup"))
                File.Delete(".bak_key_backup");

            if (IOProxy.Exists(".key"))
                File.Move(IOProxy.FileNameToCode(".key"), ".key_backup");
            if (IOProxy.Exists(".bak_key"))
                File.Move(IOProxy.FileNameToCode(".bak_key"), ".bak_key_backup");
        }

        [TestMethod]
        public void ValidateNewPasswordReEnterTest()
        {
            SecureString string1 = new SecureString();
            foreach (char c in validMasterPassword)
                string1.AppendChar(c);
            Authentification.NewMasterPassword(string1, true);

            Assert.IsFalse(viewModel.OnSubmitPassword());
            CheckUIErrors("Current password cannot be empty!", Visibility.Visible);

            viewModel.CheckPassword = new PasswordString(string1);
            Assert.IsFalse(viewModel.OnSubmitPassword());
            CheckUIErrors("", Visibility.Hidden, "New password cannot be empty!", Visibility.Visible);

            viewModel.NewPassword = new PasswordString(string1);
            Assert.IsFalse(viewModel.OnSubmitPassword());
            CheckUIErrors("", Visibility.Hidden, "Cannot use current password as a new one!", Visibility.Visible);

            SecureString string2 = new SecureString();
            foreach (char c in newPassword)
                string2.AppendChar(c);
            viewModel.NewPassword = new PasswordString(string2);
            Assert.IsFalse(viewModel.OnSubmitPassword());
            CheckUIErrors("", Visibility.Hidden, "", Visibility.Hidden, "Please re-enter new password", Visibility.Visible);

            SecureString string3 = new SecureString();
            foreach (char c in wrongPassword1)
                string3.AppendChar(c);
            viewModel.NewPasswordReEnter = new PasswordString(string3);
            Assert.IsFalse(viewModel.OnSubmitPassword());
            CheckUIErrors("", Visibility.Hidden, "Passwords don't match!", Visibility.Visible, "Passwords don't match!", Visibility.Visible);

            viewModel.CheckPassword = new PasswordString(string3);
            viewModel.NewPasswordReEnter = new PasswordString(string2);
            Assert.IsFalse(viewModel.OnSubmitPassword());
            CheckUIErrors("Current password is incorrect!", Visibility.Visible);

            SecureString string4 = new SecureString();
            foreach (char c in wrongPassword2)
                string4.AppendChar(c);
            viewModel.CheckPassword = new PasswordString(string4);
            Assert.IsFalse(viewModel.OnSubmitPassword());
            CheckUIErrors("Current password is incorrect!", Visibility.Visible);

            viewModel.CheckPassword = new PasswordString(string1);
            Assert.IsTrue(viewModel.OnSubmitPassword());
            CheckUIErrors();
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            if (IOProxy.Exists(".key"))
                File.Delete(IOProxy.FileNameToCode(".key"));
            if (IOProxy.Exists(".bak_key"))
                File.Delete(IOProxy.FileNameToCode(".bak_key"));

            File.Move(".key_backup", IOProxy.FileNameToCode(".key"));
            File.Move(".bak_key_backup", IOProxy.FileNameToCode(".bak_key"));

            Directory.SetCurrentDirectory(_currentDir);
            Application.Current.Shutdown();
        }

        private void CheckUIErrors(string checkError = "", Visibility checkErrorVisibility = Visibility.Hidden, 
            string newError = "", Visibility newVisibility = Visibility.Hidden, string reEnterError = "", 
            Visibility reEnterVisibility = Visibility.Hidden)
        {
            Assert.AreEqual(checkError, viewModel.CheckPasswordError);
            Assert.AreEqual(checkErrorVisibility, viewModel.CheckPasswordErrorFrame);
            Assert.AreEqual(newError, viewModel.NewPasswordError);
            Assert.AreEqual(newVisibility, viewModel.NewPasswordErrorFrame);
            Assert.AreEqual(reEnterError, viewModel.NewPasswordReEnterError);
            Assert.AreEqual(reEnterVisibility, viewModel.NewPasswordReEnterErrorFrame);
        }
    }
}
