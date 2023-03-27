using System.Text;

namespace PdbExtractor
{
    abstract class BytesWrapper
    {
        protected const int WORD = 2;
        protected const int DWORD = 4;
        FileStream fileStream;
        BinaryReader binaryReader;
        protected BytesWrapper(FileStream fileStream)
        {
            this.fileStream = fileStream;
            this.binaryReader = new BinaryReader(fileStream);
        }

        /* For the convinience I wrote this similar functions separately * */
        protected int parseInt(int pos)
        {
            fileStream.Seek(pos, SeekOrigin.Begin);
            return binaryReader.ReadInt32();
        }

        protected int parseInt(byte[] bytes, int pos)
        {
            return BitConverter.ToInt32(new ArraySegment<byte>(bytes, pos, count: DWORD).ToArray(), 0);
        }

        protected short parseShort(int pos)
        {
            fileStream.Seek(pos, SeekOrigin.Begin);
            return binaryReader.ReadInt16();
        }

        protected short parseShort(byte[] bytes, int pos)
        {
            return BitConverter.ToInt16(new ArraySegment<byte>(bytes, pos, count: WORD).ToArray(), 0);
        }

        protected uint parseUInt(int pos)
        {
            fileStream.Seek(pos, SeekOrigin.Begin);
            return binaryReader.ReadUInt32();
        }

        protected uint parseUInt(byte[] bytes, int pos)
        {
            return BitConverter.ToUInt32(new ArraySegment<byte>(bytes, pos, count: DWORD).ToArray(), 0);
        }

        protected ushort parseUShort(int pos)
        {
            fileStream.Seek(pos, SeekOrigin.Begin);
            return binaryReader.ReadUInt16();
        }

        protected ushort parseUShort(byte[] bytes, int pos)
        {
            return BitConverter.ToUInt16(new ArraySegment<byte>(bytes, pos, count: WORD).ToArray(), 0);
        }

        protected byte[] copySubArray(int pos, int length)
        {
            fileStream.Seek(pos, SeekOrigin.Begin);
            return binaryReader.ReadBytes(length);
        }

        protected byte[] copySubArray(byte[] bytes, int pos, int length)
        {
            byte[] copiedBytes = new byte[length];
            Array.Copy(bytes, pos, copiedBytes, 0, length);
            return copiedBytes;
        }

        protected string hexSubArray(int pos, int length)
        {
            fileStream.Seek(pos, SeekOrigin.Begin);
            return String.Join("", binaryReader.ReadBytes(length).Select(c => Convert.ToString(c, 16)));
        }

        protected string hexSubArray(byte[] bytes, int pos, int length)
        {
            return String.Join("", copySubArray(bytes, pos, length).Select(c => Convert.ToString(c, 16)));
        }

        protected string parseGUID(int pos)
        {
            fileStream.Seek(pos, SeekOrigin.Begin);
            Guid guid = new Guid(binaryReader.ReadBytes(DWORD * 4));
            return guid.ToString();
        }

        protected string parseGUID(byte[] bytes, int pos)
        {
            Guid guid = new Guid(copySubArray(bytes, pos, DWORD * 4));
            return guid.ToString();
        }

    }
}
