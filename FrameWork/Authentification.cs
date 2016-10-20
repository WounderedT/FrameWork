using FrameWork.DataModels;
using FrameWork.UC;
using FrameWork.ViewModel;
using Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Windows;
using System.Windows.Controls;

namespace FrameWork
{
    static class Authentification
    {
        /*This collection will only hold one item - authentification tab. But it must be a collection in order to 
         * use the same syntax as Session.
         */
        public static ObservableCollection<ClosableTab> Tabs { get; set; }
        public static TabItem SelectedTab { get; set; }

        public static event AuthentificationCompleteEventHandler AuthentificationComplete;
        public delegate void AuthentificationCompleteEventHandler(AuthentificationEventArgs args);

        private static PasswordObject _appPassword;

        private static ICrypto cryptography;
        private static string CryptoLibPath;// = @"F:\SkyDrive\Study\C# Projects\FrameWork\Crypto\bin\Debug\Crypto.dll";
        //private static string CryptoLibPath = @"H:\\OneDrive\Study\C# Projects\FrameWork\Crypto\bin\Debug\Crypto.dll";


        private const int _appPasswordLenght = 25;

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
            CryptoLibPath = Path.Combine(IOProxy.GetCurrentProjectFolder(), @"Crypto\bin\Debug\Crypto.dll");
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
            {
                throw new NullReferenceException("Failed to initialize cryptography object from " + CryptoLibPath + "!");
            }
        }


        public static void GetAuthentificationTab()
        {
            Tabs = new ObservableCollection<ClosableTab>();
            ClosableTab authTab = new ClosableTab(false) { Title = "Authentification" };
            if(IOProxy.Exists(".key"))
            {
                authTab.Content = new CheckPasswordView();
            }
            else
            {
                authTab.Content = new NewPasswordView();
            }
            Tabs.Add(authTab);
            SelectedTab = Tabs.First();
        }
     
        public static void NewMasterPassword(SecureString password)
        {
            EncryptedPassword result = new EncryptedPassword(cryptography.EncryptMasterPassword(password));
            if (!IOProxy.WritePassword(result, ".key"))
                OnAuthentificationComplete(new AuthentificationEventArgs(false));
            GenerateAppPassword();
            result = new EncryptedPassword(cryptography.EncryptAppPassword(_appPassword.Password, password));
            _appPassword.Salt = result.Salt;
            if (!IOProxy.WritePassword(result, ".bak_key"))
                OnAuthentificationComplete(new AuthentificationEventArgs(false));
            OnAuthentificationComplete(new AuthentificationEventArgs());
        }

        public static bool CheckMasterPassword(SecureString password, bool shortCheck = false)
        {
            EncryptedPassword created = new EncryptedPassword();
            created.GetPasswordFromFile(".key");
            int iterations = cryptography.GenerateIterationsFromSalt(created.Salt);
            if(iterations == 0)
            {
                throw new SecurityException("Password hash was tampered with!");
            }
            byte[] newHash = cryptography.GenerateMasterPasswordHash(password, created.Salt, iterations);
            if (!ConstantTimeComparison(newHash, created.Hash))
            {
                return false;
            }
            else
            {
                if (shortCheck)
                    return true;
                if (IOProxy.Exists(".bak_key"))
                {
                    EncryptedPassword appHash = new EncryptedPassword();
                    appHash.GetPasswordFromFile(".bak_key");
                    _appPassword = new PasswordObject(cryptography.DecryptAppPassword(appHash.Hash, _appPasswordLenght, password, appHash.Salt), appHash.Salt);
                }
                else
                {
                    GenerateAppPassword();
                    EncryptedPassword result = new EncryptedPassword(cryptography.EncryptAppPassword(_appPassword.Password, password));
                    _appPassword.Salt = result.Salt;
                    if (IOProxy.WritePassword(result, ".bak_key"))
                        OnAuthentificationComplete(new AuthentificationEventArgs(false));
                }
                OnAuthentificationComplete(new AuthentificationEventArgs());
                return true;
            }
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

        private static bool ConstantTimeComparison(byte[] result, byte[] created)
        {
            uint difference = (uint)result.Length ^ (uint)created.Length;
            for (var i = 0; i < result.Length && i < created.Length; i++)
            {
                difference |= (uint)(result[i] ^ created[i]);
            }
            return difference == 0;
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
