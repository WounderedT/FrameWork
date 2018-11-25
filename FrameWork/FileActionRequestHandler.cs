using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FrameWork
{
    class FileActionRequestHandler
    {
        public async Task<byte[]> ReadBytesFromFileAsync(string filename)
        {
            MemoryStream encryptedStream = await IOProxy.GetMemoryStreamFromFileAsync(filename).ConfigureAwait(false);
            if (encryptedStream == null)
                return null;
            return await Authentification.Cryptography.DecryptMemoryStreamAsync(encryptedStream,
                Authentification.AppPassword.Password, Authentification.AppPassword.Salt);
        }

        public async Task WriteBytesToFileAsync(string filename, byte[] data)
        {
            byte[] array = await Authentification.Cryptography.EncryptDataArrayAsync(data, 
                Authentification.AppPassword.Password, Authentification.AppPassword.Salt).ConfigureAwait(false);
            await IOProxy.WriteBytesToFileAsync(array, filename).ConfigureAwait(false);
        }
    }
}
