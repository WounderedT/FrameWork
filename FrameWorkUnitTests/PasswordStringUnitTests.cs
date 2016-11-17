using FrameWork.DataModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FrameWorkUnitTests
{
    [TestClass]
    public class PasswordStringUnitTests
    {
        private readonly string validPassword = "validPassword";
        private readonly string wrongPassword1 = "wrongPasswor";
        private readonly string wrongPassword2 = "wrongPassword";

        [TestMethod]
        public void EqualsTest()
        {
            PasswordString passwd1 = new PasswordString();
            Assert.IsFalse(passwd1.Equals(null));
            Assert.IsTrue(passwd1.Equals(passwd1));

            PasswordString passwd2 = new PasswordString();
            Assert.IsTrue(passwd1.Equals(passwd2));

            SecureString secureString = new SecureString();
            foreach(char c in validPassword.ToCharArray())
                secureString.AppendChar(c);
            passwd1.Password = secureString;
            Assert.IsFalse(passwd1.Equals(passwd2));

            SecureString secureString1 = new SecureString();
            foreach (char c in wrongPassword1.ToCharArray())
                secureString1.AppendChar(c);
            passwd2.Password = secureString1;
            Assert.IsFalse(passwd1.Equals(passwd2));

            SecureString secureString2 = new SecureString();
            foreach (char c in wrongPassword2.ToCharArray())
                secureString2.AppendChar(c);
            passwd2.Password = secureString2;
            Assert.IsFalse(passwd1.Equals(passwd2));

            SecureString secureString3 = new SecureString();
            foreach (char c in validPassword.ToCharArray())
                secureString3.AppendChar(c);
            passwd2.Password = secureString3;
            Assert.IsTrue(passwd1.Equals(passwd2));
        }
    }
}
