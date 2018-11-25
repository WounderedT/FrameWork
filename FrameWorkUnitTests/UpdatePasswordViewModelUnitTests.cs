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
            CommonInit.Init();
            CommonInit.LoadSettings();
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            CommonInit.CleanUp();
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
