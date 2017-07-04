using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fluctus.Format
{
    public class FileIndex : List<FileInfo>
    {
        public int FileCount { get; internal set; }

        public int Length
        {
            get
            {
                return this.Aggregate(0, (current, fileInfo) => current + fileInfo.Length);
            }
        }

        internal FileIndex() { }

        public static FileIndex Parse(BinaryReader binaryReader)
        {
            var fileIndex = new FileIndex
            {
                FileCount = binaryReader.ReadInt32() 
            };

            for (var i = 0; i < fileIndex.FileCount; i++)
            {
                var fileInfo = FileInfo.Parse(binaryReader);
                fileIndex.Add(fileInfo);
            }
            return fileIndex;
        }

        internal void Write(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(FileCount);
            foreach (var fileInfo in this)
            {
                fileInfo.Write(binaryWriter);
            }
        }
    }
}
