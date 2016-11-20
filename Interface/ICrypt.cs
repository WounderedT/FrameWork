using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;

namespace Interface
{
    public interface ICrypto
    {
        //
        // Summary:
        //     Encrypts supplied password and returns tuple of encrypted password hash and generated salt hash.
        //
        // Parameters:
        //   masterPassword:
        //     A SecureString containing master password.
        //
        // Returns:
        //     A tuple of two byte arrays which respresents enctypted password hash and generated salt hash.
        Tuple<byte[], byte[]> EncryptMasterPassword(SecureString masterPassword);
        //
        // Summary:
        //     Encrypts supplied application password and returns tuple of encrypted password hash and generated 
        //     salt hash.
        //
        // Parameters:
        //   appPassword:
        //     A SecureString containing application password.
        //
        //   key:
        //     A SecureString containing master password to be used as encryption key.
        //
        // Returns:
        //     A tuple of two byte arrays which respresents enctypted application password hash and generated
        //     salt hash.
        Tuple<byte[], byte[]> EncryptAppPassword(SecureString appPassword, SecureString key);
        //
        // Summary:
        //     Decrypts application password from supplied encrypted hash and returns SecureString.
        //
        // Parameters:
        //   hash:
        //     A byte array containig encrypted application password hash.
        //
        //   lenght:
        //     An interger containig lenght of application password.
        //
        //   masterPassword:
        //     A SecureString containig master password to be used as decryption key.
        //
        //   salt:
        //     A byte array containing salt hash which was used for application password encryption.
        //
        // Returns:
        //     A SecureString containing application password.
        //
        // Exceptions:
        //   T:System.Security.SecurityException:
        //     Application password decryption failed. 
        SecureString DecryptAppPassword(byte[] hash, int lenght, SecureString masterPassword, byte[] salt);
        //
        // Summary:
        //     Asynchronously encrypts supplied data array with supplied password.
        //
        // Parameters:
        //   hash:
        //     A byte array containig data to be encrypted.
        //
        //   password:
        //     A SecureString containig application password to be used as encryption key.
        //
        //   salt:
        //     A byte array containing salt hash.
        //
        // Returns:
        //     A task that represents the asynchronous encryption operation. The value of the TResult
        //     parameter contains a byte array with encrypted data hash.
        Task<byte[]> EncryptDataArrayAsync(byte[] data, SecureString password, byte[] salt);
        //
        // Summary:
        //     Asynchronously decrypts supplied memory stream with supplied password.
        //
        // Parameters:
        //   enctyptedStream:
        //     A memory stream containig encrypted data to be decrypted.
        //
        //   password:
        //     A SecureString containig application password to be used as decryption key.
        //
        //   salt:
        //     A byte array containing salt hash which was used for encryption.
        //
        // Returns:
        //     A task that represents the asynchronous decryption operation. The value of the TResult
        //     parameter contains a byte array containing decrypted data.
        //
        // Exceptions:
        //   T:System.Security.SecurityException:
        //     Memory stream decryption failed. 
        Task<byte[]> DecryptMemoryStreamAsync(MemoryStream enctyptedStream, SecureString password, byte[] salt);
        //
        // Summary:
        //     Decrypts supplied memory stream with supplied password.
        //
        // Parameters:
        //   enctyptedStream:
        //     A memory stream containig encrypted data to be decrypted.
        //
        //   password:
        //     A SecureString containig application password to be used as decryption key.
        //
        //   salt:
        //     A byte array containing salt hash which was used for encryption.
        //
        // Returns:
        //     A byte array containing decrypted data.
        //
        // Exceptions:
        //   T:System.Security.SecurityException:
        //     Memory stream decryption failed. 
        byte[] DecryptMemoryStream(MemoryStream enctyptedStream, SecureString password, byte[] salt);
        //
        // Summary:
        //     Generates random data to be used as salt for encryption/decryption operations.
        //
        // Returns:
        //     A byte array containing salt hash.
        byte[] GenerateSalt();
        //
        // Summary:
        //     Generates iterations number based on supplied salt hash.
        //
        // Parameters:
        //     A byte array containing salt hash.
        //
        // Returns:
        //     An integer representing number of encryption/decryption iterations. This number should not be 
        //     less than 12000.
        int GenerateIterationsFromSalt(byte[] salt);
        //
        // Summary:
        //     Generates hash for supplied master password.
        //
        // Parameters:
        //   password:
        //     A SecureString containig master password.
        //
        //   salt:
        //     A byte array containing salt hash which was used for encryption.
        //
        //   iterations:
        //     An interger representing number of iterations for hash generation.
        //
        // Returns:
        //     A byte array containing master password hash.
        byte[] GenerateMasterPasswordHash(SecureString password, byte[] salt, int iterations);
    }
}
