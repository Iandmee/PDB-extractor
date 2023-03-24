using System.Text;

namespace PdbExtractor
{
    abstract class BytesWrapper
    {
        protected const int WORD = 2;
        protected const int DWORD = 4;
        private byte[] rawBytes;
        protected BytesWrapper(byte[] bytes)
        {
            rawBytes = bytes;
        }

        /* For the convinience I wrote this similar functions separately * */
        protected int parseInt(int pos)
        {
            return parseInt(rawBytes, pos);
        }

        protected int parseInt(byte[] bytes, int pos)
        {
            return BitConverter.ToInt32(new ArraySegment<byte>(bytes, pos, count: DWORD).ToArray(), 0);
        }

        protected short parseShort(int pos)
        {
            return parseShort(rawBytes, pos);
        }

        protected short parseShort(byte[] bytes, int pos)
        {
            return BitConverter.ToInt16(new ArraySegment<byte>(bytes, pos, count: WORD).ToArray(), 0);
        }

        protected uint parseUInt(int pos)
        {
            return parseUInt(rawBytes, pos);
        }

        protected uint parseUInt(byte[] bytes, int pos)
        {
            return BitConverter.ToUInt32(new ArraySegment<byte>(bytes, pos, count: DWORD).ToArray(), 0);
        }

        protected ushort parseUShort(int pos)
        {
            return parseUShort(rawBytes, pos);
        }

        protected ushort parseUShort(byte[] bytes, int pos)
        {
            return BitConverter.ToUInt16(new ArraySegment<byte>(bytes, pos, count: WORD).ToArray(), 0);
        }

        protected byte[] copySubArray(int pos, int length)
        {
            return copySubArray(rawBytes, pos, length);
        }

        protected byte[] copySubArray(byte[] bytes, int pos, int length)
        {
            byte[] copiedBytes = new byte[length];
            Array.Copy(bytes, pos, copiedBytes, 0, length);
            return copiedBytes;
        }

        protected string hexSubArray(int pos, int length)
        {
            return hexSubArray(rawBytes, pos, length);
        }

        protected string hexSubArray(byte[] bytes, int pos, int length)
        {
            return String.Join("", copySubArray(bytes, pos, length).Select(c => Convert.ToString(c, 16)));
        }

        protected string reversedHexSubArray(int pos, int length)
        {
            return reversedHexSubArray(rawBytes, pos, length);
        }

        protected string reversedHexSubArray(byte[] bytes, int pos, int length)
        {
            return String.Join("", copySubArray(bytes, pos, length).Select(c => Convert.ToString(c, 16)).Reverse());
        }

        protected string parseGUID(int pos)
        {
            return parseGUID(rawBytes, pos);
        }

        protected string parseGUID(byte[] bytes, int pos)
        {
            StringBuilder guid = new StringBuilder();
            var offset = 0;
            guid.Append(reversedHexSubArray(bytes, offset + pos, DWORD)).Append("-");
            offset += DWORD;
            guid.Append(reversedHexSubArray(bytes, offset + pos, WORD)).Append("-");
            offset += WORD;
            guid.Append(reversedHexSubArray(bytes, offset + pos, WORD)).Append("-");
            offset += WORD;
            guid.Append(hexSubArray(bytes, offset + pos, DWORD * 2));
            return guid.ToString();
        }

    }
}
