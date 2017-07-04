using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Fluctus.Format
{
    public class DataSegment : List<byte[]>
    {
        public int Length
        {
            get { return this.Aggregate(0, (current, data) => current + data.Length); }
        }

        internal DataSegment()
        {

        }

        internal int AddFile(byte[] fileBytes, string encryptionPassword)
        {
            int encryptedLength;
            Add(Encrypt(fileBytes, encryptionPassword, out encryptedLength));

            return encryptedLength;
        }

        internal byte[] GetFile(int index, string decryptionPassword, int encryptedLength)
        {
            return Decrypt(this[index], decryptionPassword, encryptedLength);
        }

        internal void Write(BinaryWriter binaryWriter)
        {
            foreach (var fileData in this)
            {
                binaryWriter.Write(fileData);
            }
        }

        private byte[] Encrypt(byte[] array, string password, out int encryptedLength)
        {
            var passwordDeriveBytes = new Rfc2898DeriveBytes(password, new byte[] { 0x00, 0x44, 0x12, 0x6E, 0x20, 0x4F, 0xFF, 0x65, 0x11, 0x22, 0x12, 0x13 });

            var tripleDes = new TripleDESCryptoServiceProvider
            {
                Key = passwordDeriveBytes.GetBytes(24),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = passwordDeriveBytes.GetBytes(8)
            };

            var cryptoTransform = tripleDes.CreateEncryptor();
            var resultingEncryptedArray = cryptoTransform.TransformFinalBlock(array, 0, array.Length);

            tripleDes.Clear();

            encryptedLength = resultingEncryptedArray.Length;
            return resultingEncryptedArray;
        }

        private byte[] Decrypt(byte[] array, string password, int encryptedLength)
        {
            var passwordDeriveBytes = new Rfc2898DeriveBytes(password, new byte[] { 0x00, 0x44, 0x12, 0x6E, 0x20, 0x4F, 0xFF, 0x65, 0x11, 0x22, 0x12, 0x13 });

            var tripleDes = new TripleDESCryptoServiceProvider
            {
                Key = passwordDeriveBytes.GetBytes(24),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = passwordDeriveBytes.GetBytes(8)
            };

            var cryptoTransform = tripleDes.CreateDecryptor();
            var resultingDecryptedArray = cryptoTransform.TransformFinalBlock(array, 0, encryptedLength);

            tripleDes.Clear();

            return resultingDecryptedArray;
        }
    }
}
