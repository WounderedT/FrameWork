using FrameWork.DataModels;
using FrameWork.UC;
using FrameWork.ViewModel;
using Interface;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Web.Security;
using System.Windows;

namespace FrameWork
{
    public static class Authentification
    {
        private const int _appPasswordLenght = 25;

        private static PasswordObject _appPassword;
        private static ICrypto cryptography;
        private static string CryptoLibPath;

        public static event AuthentificationCompleteEventHandler AuthentificationComplete;
        public delegate void AuthentificationCompleteEventHandler(AuthentificationEventArgs args);


        public static PasswordObject AppPassword
        {
            get { return _appPassword; }
        }

        public static ICrypto Cryptography
        {
            get { return cryptography; }
        }

        static Authentification()
        {
            CryptoLibPath = Path.Combine(Directory.GetCurrentDirectory(), "Crypto.dll");
            var DLL = Assembly.LoadFile(CryptoLibPath);
            foreach (Type type in DLL.GetExportedTypes())
            {
                try
                {
                    cryptography = (ICrypto)Activator.CreateInstance(type);
                    break;
                }
                catch (InvalidCastException) { }
            }
            if (cryptography == null)
                throw new NullReferenceException("Failed to initialize cryptography object from " + CryptoLibPath + "!");
        }


        public static void GetAuthentificationTab()
        {
            ClosableTab authTab = new ClosableTab(false) { Title = "Authentification" };
            if(IOProxy.Exists(".key"))
            {
                authTab.Content = new CheckPasswordView();
            }
            else
            {
                authTab.Content = new NewPasswordView();
            }
            MainWindowViewModel.tabs.Add(authTab);
        }
     
        public static void NewMasterPassword(SecureString password, bool shortCheck = false)
        {
            EncryptedPassword result = new EncryptedPassword(cryptography.EncryptMasterPassword(password));
            if (!IOProxy.WritePassword(result, ".key"))
                OnAuthentificationComplete(new AuthentificationEventArgs(false));
            NewApplicationPassword(password);
            if (shortCheck)
                return;
            OnAuthentificationComplete(new AuthentificationEventArgs());
        }

        public static bool CheckMasterPassword(SecureString password, bool shortCheck = false)
        {
            EncryptedPassword created = new EncryptedPassword();
            created.GetPasswordFromFile(".key");
            int iterations = cryptography.GenerateIterationsFromSalt(created.Salt);
            if(iterations == 0)
                throw new SecurityException("Password hash was tampered with!");
            byte[] newHash = cryptography.GenerateMasterPasswordHash(password, created.Salt, iterations);
            if (!ConstantTimeComparison(newHash, created.Hash))
                return false;
            else
            {
                string commonErrorMessage = "New password could be generated but all existing data including plugins state will be lost."
                    + Environment.NewLine + "Would you like to generate new password?";
                if (shortCheck)
                    return true;
                if (IOProxy.Exists(".bak_key"))
                {
                    EncryptedPassword appHash = new EncryptedPassword();
                    appHash.GetPasswordFromFile(".bak_key");
                    try
                    {
                        _appPassword = new PasswordObject(cryptography.DecryptAppPassword(appHash.Hash, _appPasswordLenght, password, appHash.Salt), appHash.Salt);
                    }
                    catch (Exception)
                    {
                        if (!ShowAppPasswordDecryptionError("Application password cannot be decrypted! " + Environment.NewLine + commonErrorMessage))
                            OnAuthentificationComplete(new AuthentificationEventArgs(false));
                        else
                            NewApplicationPassword(password);
                    }
                }
                else
                {
                    if(!ShowAppPasswordDecryptionError("Application password file not found! " + Environment.NewLine + commonErrorMessage))
                        OnAuthentificationComplete(new AuthentificationEventArgs(false));
                    else
                        NewApplicationPassword(password);
                }
                OnAuthentificationComplete(new AuthentificationEventArgs());
                return true;
            }
        }

        public static void NewApplicationPassword(SecureString password)
        {
            GenerateAppPassword();
            EncryptedPassword result = new EncryptedPassword(cryptography.EncryptAppPassword(_appPassword.Password, password));
            _appPassword.Salt = result.Salt;
            if (!IOProxy.WritePassword(result, ".bak_key"))
                OnAuthentificationComplete(new AuthentificationEventArgs(false));
        }

        private static bool ConstantTimeComparison(byte[] result, byte[] created)
        {
            uint difference = (uint)result.Length ^ (uint)created.Length;
            for (var i = 0; i < result.Length && i < created.Length; i++)
            {
                difference |= (uint)(result[i] ^ created[i]);
            }
            return difference == 0;
        }

        private static void GenerateAppPassword(bool force = false)
        {
            if (_appPassword != null)
                if (_appPassword.isValid() && !force)
                    return;
            _appPassword = new PasswordObject();
            foreach(char c in Membership.GeneratePassword(_appPasswordLenght, 7))
            {
                _appPassword.Password.AppendChar(c);
            }
        }

        private static bool ShowAppPasswordDecryptionError(string message)
        {
            string caption = "Password decryption error";
            var result = MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.YesNo, MessageBoxImage.Error);
            return result.Equals(MessageBoxResult.Yes) ? true : false;
        }

        private static void OnAuthentificationComplete(AuthentificationEventArgs args)
        {
            AuthentificationComplete?.Invoke(args);
        }
    }

    [Serializable]
    public class EncryptedPassword
    {
        public byte[] Hash
        {
            get; set;
        }

        public byte[] Salt
        {
            get; set;
        }

        public EncryptedPassword() { }

        public EncryptedPassword(Tuple<byte[], byte[]> result)
        {
            Hash = result.Item1;
            Salt = result.Item2;
        }

        public void GetPasswordFromFile(string filename)
        {
            MemoryStream ms = IOProxy.GetMemoryStreamFromFile(filename);
            EncryptedPassword ps = new EncryptedPassword();
            BinaryFormatter formatter = new BinaryFormatter();
            ps = (EncryptedPassword)formatter.Deserialize(ms);
            Hash = ps.Hash;
            Salt = ps.Salt;
        }
    }

    public class PasswordObject
    {
        public SecureString Password
        {
            get; set;
        }

        public byte[] Salt
        {
            get; set;
        }

        public PasswordObject()
        {
            Password = new SecureString();
        }

        public PasswordObject(SecureString password, byte[] salt)
        {
            Password = password;
            Salt = salt;
        }

        public bool isValid()
        {
            if (Password == null || Salt == null)
                return false;
            if (Password.Length == 0 || Salt.Length == 0)
                return false;
            return true;
        }
    }

    public class AuthentificationEventArgs: EventArgs
    {
        public bool Result { get; set; }

        public AuthentificationEventArgs() : this(true) { }

        public AuthentificationEventArgs(bool result)
        {
            Result = result;
        }
    }
}
