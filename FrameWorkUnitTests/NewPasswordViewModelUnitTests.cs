using FrameWork.DataModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security;
using System.Windows;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class NewPasswordViewModelUnitTests
    {
        private readonly string validPassword = "1";
        private readonly string wrongPassword = "2";

        private FrameWork.ViewModel.NewPasswordViewModel viewModel = new FrameWork.ViewModel.NewPasswordViewModel();

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
        public void ValidateNewPasswordTest()
        {
            viewModel.OnSubmitPassword();
            Assert.AreEqual("Password cannot be empty!", viewModel.PasswordError);
            Assert.AreEqual(Visibility.Visible, viewModel.NewPasswordErrorFrame);

            SecureString passwd = new SecureString();
            foreach(char c in validPassword.ToCharArray())
                passwd.AppendChar(c);
            viewModel.NewPassword = new PasswordString(passwd);
            viewModel.OnSubmitPassword();
            Assert.AreEqual("Please re-enter the password", viewModel.PasswordError);
            Assert.AreEqual(Visibility.Hidden, viewModel.NewPasswordErrorFrame);
            Assert.AreEqual(Visibility.Visible, viewModel.NewPasswordReEnterErrorFrame);

            Assert.AreNotEqual(string.Empty, viewModel.PasswordTips);
        }

        [TestMethod]
        public void ValidateNewPasswordReEnterTest()
        {
            SecureString passwd = new SecureString();
            foreach (char c in validPassword.ToCharArray())
                passwd.AppendChar(c);
            viewModel.NewPassword = new PasswordString(passwd);

            viewModel.OnSubmitPassword();
            Assert.AreEqual("Please re-enter the password", viewModel.PasswordError);
            Assert.AreEqual(Visibility.Hidden, viewModel.NewPasswordErrorFrame);
            Assert.AreEqual(Visibility.Visible, viewModel.NewPasswordReEnterErrorFrame);

            SecureString passwdWrong = new SecureString();
            foreach (char c in wrongPassword.ToCharArray())
                passwdWrong.AppendChar(c);
            viewModel.NewPasswordReEnter = new PasswordString(passwdWrong);
            viewModel.OnSubmitPassword();
            Assert.AreEqual("Passwords don't match!", viewModel.PasswordError);
            Assert.AreEqual(Visibility.Hidden, viewModel.NewPasswordErrorFrame);
            Assert.AreEqual(Visibility.Visible, viewModel.NewPasswordReEnterErrorFrame);

            SecureString passwdValid = new SecureString();
            foreach (char c in validPassword.ToCharArray())
                passwdValid.AppendChar(c);
            viewModel.NewPasswordReEnter = new PasswordString(passwdValid);
            try
            {
                viewModel.OnSubmitPassword();
            }
            catch (TypeInitializationException) { }
            Assert.AreEqual(string.Empty, viewModel.PasswordError);
            Assert.AreEqual(Visibility.Hidden, viewModel.NewPasswordErrorFrame);
            Assert.AreEqual(Visibility.Hidden, viewModel.NewPasswordReEnterErrorFrame);
        }
    }
}
