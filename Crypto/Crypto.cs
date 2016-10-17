using Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypto
{
    public class Crypto: ICrypto
    {
        private const int _iterationsThreshold = 12000;
        private const int _saltSize = 24;
        private const int _masterPasswordKeyLength = 32;

        public Tuple<byte[], byte[]> EncryptMasterPassword(SecureString masterPassword)
        {
            Tuple<byte[], int> parameters = GetPasswordEncryptionParameters();
            byte[] hash = GenerateMasterPasswordHash(masterPassword, parameters.Item1, parameters.Item2);
            return new Tuple<byte[], byte[]>(hash, parameters.Item1);
        }

        public Tuple<byte[], byte[]> EncryptAppPassword(SecureString appPassword, SecureString masterPassword)
        {
            Tuple<byte[], int> parameters = GetPasswordEncryptionParameters();
            byte[] key = GeneratePasswordHash(masterPassword, parameters.Item1, parameters.Item2, 16);
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(appPassword);
                byte[] passwd = new byte[appPassword.Length * sizeof(char)];
                Buffer.BlockCopy(Marshal.PtrToStringUni(unmanagedString).ToCharArray(), 0, passwd, 0, passwd.Length);
                using (Aes algorithm = Aes.Create())
                using (ICryptoTransform encryptor = algorithm.CreateEncryptor(key, key))
                    return new Tuple<byte[], byte[]>(Crypt(passwd, encryptor), parameters.Item1);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public SecureString DecryptAppPassword(byte[] hash, int lenght, SecureString masterPassword, byte[] salt)
        {
            byte[] key = GeneratePasswordHash(masterPassword, salt, GenerateIterationsFromSalt(salt), 16);
            SecureString passString = new SecureString();
            byte[] array = new byte[lenght * sizeof(char)];
            try
            {
                using (Aes algorithm = Aes.Create())
                using (ICryptoTransform decryptor = algorithm.CreateDecryptor(key, key))
                    array = Crypt(hash, decryptor);
            }
            catch (CryptographicException)
            {
                throw new SecurityException("Password hash was tampered with!");
            }
            char[] chars = new char[lenght];
            Buffer.BlockCopy(array, 0, chars, 0, array.Length);
            foreach (char c in chars)
                passString.AppendChar(c);
            return passString;
        }

        public async Task<byte[]> EncryptDataArrayAsync(byte[] data, SecureString password, byte[] salt)
        {
            byte[] key = await GeneratePasswordHashAsync(password, salt, GenerateIterationsFromSalt(salt), 16);
            using (Aes algorithm = Aes.Create())
            using (ICryptoTransform encryptor = algorithm.CreateEncryptor(key, key))
                return await CryptAsync(data, encryptor);
        }

        public async Task<byte[]> DecryptMemoryStreamAsync(MemoryStream enctyptedStream, SecureString password, byte[] salt)
        {
            byte[] key = await GeneratePasswordHashAsync(password, salt, GenerateIterationsFromSalt(salt), 16);
            try
            {
                using (Aes algorithm = Aes.Create())
                using (ICryptoTransform decryptor = algorithm.CreateDecryptor(key, key))
                    return await CryptAsync(enctyptedStream.ToArray(), decryptor);
            }
            catch (CryptographicException)
            {
                throw new SecurityException("Password hash was tampered with!");
            }
        }

        public byte[] DecryptMemoryStream(MemoryStream enctyptedStream, SecureString password, byte[] salt)
        {
            byte[] key = GeneratePasswordHash(password, salt, GenerateIterationsFromSalt(salt), 16);
            try
            {
                using (Aes algorithm = Aes.Create())
                using (ICryptoTransform decryptor = algorithm.CreateDecryptor(key, key))
                    return Crypt(enctyptedStream.ToArray(), decryptor);
            }
            catch (CryptographicException)
            {
                throw new SecurityException("Password hash was tampered with!");
            }
        }

        private byte[] Crypt(byte[] data, ICryptoTransform cryptor)
        {
            MemoryStream m = new MemoryStream();
            using (Stream c = new CryptoStream(m, cryptor, CryptoStreamMode.Write))
                c.Write(data, 0, data.Length);
            return m.ToArray();
        }

        private async Task<byte[]> CryptAsync(byte[] data, ICryptoTransform cryptor)
        {
            MemoryStream m = new MemoryStream();
            using (Stream c = new CryptoStream(m, cryptor, CryptoStreamMode.Write))
                await c.WriteAsync(data, 0, data.Length);
            return m.ToArray();
        }

        private async Task<MemoryStream> CryptToStreamAsync(byte[] data, ICryptoTransform cryptor)
        {
            MemoryStream m = new MemoryStream();
            using (Stream c = new CryptoStream(m, cryptor, CryptoStreamMode.Write))
                await c.WriteAsync(data, 0, data.Length);
            var some = m.ToArray();
            return m;
        }

        private Tuple<byte[],int> GetPasswordEncryptionParameters()
        {
            int iterations = 0;
            byte[] salt = new byte[_saltSize];
            while(iterations == 0)
            {
                salt = GenerateSalt();
                iterations = GenerateIterationsFromSalt(salt);
            }
            return new Tuple<byte[], int>(salt, iterations);
        }

        public byte[] GenerateSalt()
        {
            var salt = new byte[_saltSize];
            using (var csprng = new RNGCryptoServiceProvider())
            {
                csprng.GetBytes(salt);
            }
            return salt;
        }

        public int GenerateIterationsFromSalt(byte[] salt)
        {
            int iterations = 0;
            foreach(byte b in salt)
            {
                iterations += b * 5;
            }
            if(iterations >= _iterationsThreshold)
            {
                return iterations;
            }
            else
            {
                return 0;
            }
        }

        public byte[] GenerateMasterPasswordHash(SecureString password, byte[] salt, int iterations)
        {
            return GeneratePasswordHash(password, salt, iterations, _masterPasswordKeyLength);
        }

        private async Task<byte[]> GeneratePasswordHashAsync(SecureString password, byte[] salt, int iterations, int vectorSize)
        {
            return await Task.Run(() => GeneratePasswordHash(password, salt, iterations, vectorSize));
        }

        private byte[] GeneratePasswordHash(SecureString password, byte[] salt, int iterations, int vectorSize)
        {
            byte[] hash;
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(password);
                using (var pbkdf2 = new Rfc2898DeriveBytes(Marshal.PtrToStringUni(unmanagedString), salt, iterations))
                {
                    hash = pbkdf2.GetBytes(vectorSize);
                }
                return hash;
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
