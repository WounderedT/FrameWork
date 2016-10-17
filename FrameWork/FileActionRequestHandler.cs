using Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FrameWork
{
    class FileActionRequestHandler
    {
        public async Task<byte[]> ReadBytesFromFileAsync(string filename)
        {
            if (!IOProxy.Exists(filename))
            {
                return null;
            }
            MemoryStream encryptedStream = await IOProxy.GetMemoryStreamFromFileAsync(filename);
            return await Authentification.Cryptography.DecryptMemoryStreamAsync(encryptedStream,
                Authentification.AppPassword.Password, Authentification.AppPassword.Salt);
        }

        public byte[] ReadBytesFromFile(string filename)
        {
            if (!IOProxy.Exists(filename))
            {
                return null;
            }
            MemoryStream encryptedStream = IOProxy.GetMemoryStreamFromFile(filename);
            return Authentification.Cryptography.DecryptMemoryStream(encryptedStream,
                Authentification.AppPassword.Password, Authentification.AppPassword.Salt);
        }

        public async Task WriteBytesToFileAsync(string filename, byte[] data)
        {
            byte[] array = await Authentification.Cryptography.EncryptDataArrayAsync(data, 
                Authentification.AppPassword.Password, Authentification.AppPassword.Salt);
            await IOProxy.WriteBytesToFileAsync(array, filename);
        }
    }
}
