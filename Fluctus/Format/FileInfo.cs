using System;
using System.IO;
using System.Text;

namespace Fluctus.Format
{
    public class FileInfo
    {
        public int FileIndexNumber { get; private set; }
        public int FileLength { get; private set; }
        public int FileEncryptedLength { get; private set; }
        public byte[] FileNameData { get; private set; }
        public byte[] ExtensionData { get; private set; }

        public string FileName
        {
            get
            {
                var decryptedFileName = new byte[FileNameData.Length];
                for (var i = 0; i < decryptedFileName.Length; i++)
                {
                    decryptedFileName[i] = (byte)(FileNameData[i] ^ 21);
                }
                return Encoding.UTF8.GetString(decryptedFileName).Trim('\0');
            }
        }

        public string Extension
        {
            get
            {
                var decryptedExtension = new byte[ExtensionData.Length];
                for (var i = 0; i < decryptedExtension.Length; i++)
                {
                    decryptedExtension[i] = (byte)(ExtensionData[i] ^ 21);
                }
                return Encoding.UTF8.GetString(decryptedExtension).Trim('\0');
            }
        }

        public int Length => 4*3 + FileNameData.Length + ExtensionData.Length;

        private FileInfo() { }

        public FileInfo(int fileOffset, int fileLength, int fileEncryptedLength, string fileName, string extension)
        {
            FileIndexNumber = fileOffset;
            FileLength = fileLength;
            FileEncryptedLength = fileEncryptedLength;

            var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            if (fileNameBytes.Length > 64)
            {
                throw new Exception("The file name is too long for the buffer.");
            }
            FileNameData = Encrypt(PadWithZeroes(fileNameBytes, 64), 21);

            var extensionBytes = Encoding.UTF8.GetBytes(extension);
            if (extensionBytes.Length > 16)
            {
                throw new Exception("The extension is too long for the buffer.");
            }
            ExtensionData = Encrypt(PadWithZeroes(extensionBytes, 16), 21);
        }

        public static FileInfo Parse(BinaryReader binaryReader)
        {
            var fileInfo = new FileInfo
            {
                FileIndexNumber = binaryReader.ReadInt32(),
                FileLength = binaryReader.ReadInt32(),
                FileEncryptedLength = binaryReader.ReadInt32(),
                FileNameData = binaryReader.ReadBytes(64),
                ExtensionData = binaryReader.ReadBytes(16)
            };
            return fileInfo;
        }

        internal void Write(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(FileIndexNumber);
            binaryWriter.Write(FileLength);
            binaryWriter.Write(FileEncryptedLength);
            binaryWriter.Write(FileNameData);
            binaryWriter.Write(ExtensionData);
        }

        private byte[] PadWithZeroes(byte[] array, int requiredLength)
        {
            if (array.Length < requiredLength)
            {
                var zeroCount = requiredLength - array.Length;
                var newFileNameBuffer = new byte[requiredLength];

                for (var i = 0; i < array.Length; i++)
                {
                    newFileNameBuffer[i] = array[i];
                }

                for (var i = array.Length; i < zeroCount; i++)
                {
                    newFileNameBuffer[i] = 0;
                }

                return newFileNameBuffer;
            }
            return array;
        }

        private byte[] Encrypt(byte[] array, byte key)
        {
            if (array.Length <= 0)
                return array;

            var encryptedArray = new byte[array.Length];
            for (var i = 0; i < encryptedArray.Length; i++)
            {
                encryptedArray[i] = (byte)(array[i] ^ key);
            }

            return encryptedArray;
        }
    }
}
