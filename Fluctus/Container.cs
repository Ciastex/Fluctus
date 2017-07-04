using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fluctus.Format;
using FileInfo = Fluctus.Format.FileInfo;

namespace Fluctus
{
    public class Container
    {
        private BinaryReader _binaryReader;
        private BinaryWriter _binaryWriter;
        private FileStream _fileStream;
        private readonly string _fileName;
        private string _password;

        internal const short FileVersion = 0x0002;

        public Header Header { get; private set; }
        public FileIndex Index { get; private set; }
        public DataSegment DataSegment { get; private set; }

        private Container()
        {
            
        }

        public Container(string fileName, string password)
        {
            _fileName = fileName;
            _password = password;

            Index = new FileIndex();
            DataSegment = new DataSegment();
        }

        public void AddFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);
            var encryptedLength = DataSegment.AddFile(fileBytes, _password);

            var strippedFileName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var fileInfo = new FileInfo(Index.FileCount++, fileBytes.Length, encryptedLength, strippedFileName, extension);

            Index.Add(fileInfo);
        }

        public FileInfo GetFileInfo(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);

            return Index.FirstOrDefault(x => x.FileName == name && x.Extension == ext);
        }

        public void ExtractFile(string fileName, string targetFileName)
        {
            var fileInfo = GetFileInfo(fileName);
            var index = fileInfo.FileIndexNumber;

            File.WriteAllBytes(targetFileName, DataSegment.GetFile(index, _password, fileInfo.FileEncryptedLength));
        }

        public void ExtractAll()
        {
            foreach (var fileInfo in Index)
            {
                File.WriteAllBytes($"{fileInfo.FileName}{fileInfo.Extension}", DataSegment.GetFile(fileInfo.FileIndexNumber, _password, fileInfo.FileEncryptedLength));
            }
        }

        public bool FileExists(string file)
        {
            return GetFiles().Contains(file);
        }

        public List<string> GetFiles()
        {
            return Index.Select(fileInfo => $"{fileInfo.FileName}{fileInfo.Extension}").ToList();
        }

        public void RemoveFile(string fileName)
        {
            var fileInfo = GetFileInfo(fileName);
            DataSegment.RemoveAt(fileInfo.FileIndexNumber);
        }

        public static Container Read(string fileName, string password)
        {
            var container = new Container
            {
                _fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read),
                _password = password
            };
            container._binaryReader = new BinaryReader(container._fileStream);

            container.Header = Header.Parse(container._binaryReader);
            container.Index = FileIndex.Parse(container._binaryReader);
            
            var dataSegment = new DataSegment();
            dataSegment.AddRange(container.Index.Select(fileInfo => container._binaryReader.ReadBytes(fileInfo.FileEncryptedLength)));

            container.DataSegment = dataSegment;

            return container;
        }

        public void Write()
        {
            _fileStream = new FileStream(_fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            _binaryWriter = new BinaryWriter(_fileStream);

            Header = new Header(Encoding.UTF8.GetBytes("FLUC"), FileVersion, CalculateFileLength(), CalculateFileLength() - DataSegment.Length);

            Header.Write(_binaryWriter);
            Index.Write(_binaryWriter);
            DataSegment.Write(_binaryWriter);

            _binaryWriter.Dispose();
            _fileStream.Dispose();
        }

        private int CalculateFileLength()
        {
            return Header.Length + Index.Length + DataSegment.Length;
        }
    }
}
