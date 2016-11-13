using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;

namespace Interface
{
    public interface ICrypto
    {
        Tuple<byte[], byte[]> EncryptMasterPassword(SecureString masterPassword);
        Tuple<byte[], byte[]> EncryptAppPassword(SecureString appPassword, SecureString key);
        SecureString DecryptAppPassword(byte[] hash, int lenght, SecureString masterPassword, byte[] salt);
        Task<byte[]> EncryptDataArrayAsync(byte[] data, SecureString password, byte[] salt);
        Task<byte[]> DecryptMemoryStreamAsync(MemoryStream enctyptedStream, SecureString password, byte[] salt);
        byte[] DecryptMemoryStream(MemoryStream enctyptedStream, SecureString password, byte[] salt);
        byte[] GenerateSalt();
        int GenerateIterationsFromSalt(byte[] salt);
        byte[] GenerateMasterPasswordHash(SecureString password, byte[] salt, int iterations);
    }
}
