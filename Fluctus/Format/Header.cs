using System.IO;
using System.Text;

namespace Fluctus.Format
{
    public class Header
    {
        public byte[] MagicNumber { get; private set; }
        public short FileVersion { get; private set; }
        public int FileLength { get; private set; }
        public int DataSegmentOffset { get; private set; }

        public string MagicString => Encoding.UTF8.GetString(MagicNumber);
        public static int Length => /* MagicNumber.Length + */ 2 + (4 * 3);

        internal Header() { }

        public Header(byte[] magicNumber, short fileVersion, int fileLength, int dataSegmentOffset)
        {
            MagicNumber = magicNumber;
            FileVersion = fileVersion;
            FileLength = fileLength;
            DataSegmentOffset = dataSegmentOffset;
        }

        public static Header Parse(BinaryReader binaryReader)
        {
            var hdr = new Header
            {
                MagicNumber = binaryReader.ReadBytes(4),
                FileVersion = binaryReader.ReadInt16(),
                FileLength = binaryReader.ReadInt32(),
                DataSegmentOffset = binaryReader.ReadInt32()
            };

            return hdr;
        }

        internal void Write(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(MagicNumber);
            binaryWriter.Write(FileVersion);
            binaryWriter.Write(FileLength);
            binaryWriter.Write(DataSegmentOffset);
        }
    }
}
