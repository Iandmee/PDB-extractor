using System.Runtime.InteropServices;
using System.Text;

namespace PdbExtractor
{
    internal class PdbParser : PdbExtractor.BytesWrapper
    {
        enum StreamName : int
        {
            OldDirectoryStream,
            PdbStream,
            TpiStream,
            DbiStream,
            StreamDirectory = -1
        }

        const int BLOCK_SIZE_OFFSET = 0x20;
        const int SIZE_OF_STREAM_DIRECTORY_OFFSET = 0x2c;
        const int POINTER_TO_STREAM_DIRECTORY_OFFSET = 0x34;
        const string ACCEPTED_SIGNATURE = "Microsoft C/C++ MSF 7.00";
        readonly List<Stream> streams;
        readonly List<DbiModInfoRecord> dbiModInfoRecords;
        Authentity authentity;
        DbiStreamHeader dbiHeader;
        int blockSize;
        int sizeOfStreamDirectory;
        int pointerToStreamDirectory;
        int numberOfStreamDirectoryParts;
        int[] pointersOfStreamDirectory;
        int currentOffset;

        public PdbParser(FileStream fileStream) : base(fileStream)
        {
            try
            {
                checkPdbFormat();
                currentOffset = 0;
                streams = new List<Stream>();
                dbiModInfoRecords = new List<DbiModInfoRecord>();
                getStreamDirectoryPointers();
                parseStreamDirectory();
                parseStreams();
            }
            catch
            {
                Console.Error.WriteLine("Some error occured! (probably wrong file format)");
                throw;
            }
        }

        private void checkPdbFormat()
        {
            var signature = new String(copySubArray(0, 24).Select(b => (char)b).ToArray());
            if (signature != ACCEPTED_SIGNATURE)
            {
                throw new ArgumentException("Wrong Pdb file format!");
            }
        }

        private void getStreamDirectoryPointers()
        {
            blockSize = parseShort(BLOCK_SIZE_OFFSET);
            sizeOfStreamDirectory = parseInt(SIZE_OF_STREAM_DIRECTORY_OFFSET);
            pointerToStreamDirectory = blockSize * parseInt(POINTER_TO_STREAM_DIRECTORY_OFFSET);
            numberOfStreamDirectoryParts = sizeOfStreamDirectory % blockSize == 0 ? sizeOfStreamDirectory / blockSize : sizeOfStreamDirectory / blockSize + 1;
            pointersOfStreamDirectory = new int[numberOfStreamDirectoryParts];
            for (int i = 0; i < numberOfStreamDirectoryParts; i++)
            {
                pointersOfStreamDirectory[i] = parseInt(pointerToStreamDirectory + i * DWORD) * blockSize;
            }
        }

        private void parseStreamDirectory()
        {
            var bytes = getStreamParts(StreamName.StreamDirectory);
            var numberOfStreams = parseInt(bytes, currentOffset);
            currentOffset += DWORD;
            for (int i = 0; i < numberOfStreams; i++)
            {
                var streamSize = parseInt(bytes, currentOffset);
                streams.Add(new Stream(streamSize, blockSize));
                currentOffset += DWORD;
            }
            foreach (var stream in streams)
            {
                if (stream.size == 0)
                {
                    continue;
                }
                for (int pointer = 0; pointer < stream.pointers.Length; pointer++)
                {
                    stream.pointers[pointer] = parseInt(bytes, currentOffset) * blockSize;
                    currentOffset += DWORD;
                }
            }
            currentOffset = 0;
        }

        private void parseAuthority(byte[] bytes)
        {
            var version = (PdbStreamVersion)parseInt(bytes, currentOffset);
            currentOffset += 2 * DWORD;
            var age = parseInt(bytes, currentOffset);
            currentOffset += DWORD;
            authentity = new Authentity(version.ToString(), parseGUID(bytes, currentOffset), age);
        }

        private void parsePdbStream()
        {
            var bytes = getStreamParts(StreamName.PdbStream);
            parseAuthority(bytes);

            currentOffset = 0;

        }

        private void parseDbiHeader(byte[] dbiBytes)
        {
            var pointer = streams[(int)StreamName.DbiStream].pointers[0];
            var size = Marshal.SizeOf(dbiHeader);
            IntPtr pnt = Marshal.AllocHGlobal(size);
            Marshal.Copy(dbiBytes, 0, pnt, size);
            dbiHeader = (DbiStreamHeader)Marshal.PtrToStructure(pnt, typeof(DbiStreamHeader));
        }

        private int findFirstZero(byte[] bytes, int offset)
        {
            for (int i = offset; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private DbiModInfoRecord parseDbiModInfoRecord(byte[] dbiBytes)
        {
            ModInfoFields modInfoFields = new();
            var size = Marshal.SizeOf(modInfoFields);
            IntPtr pnt = Marshal.AllocHGlobal(size);
            Marshal.Copy(dbiBytes, currentOffset, pnt, size);
            modInfoFields = (ModInfoFields)Marshal.PtrToStructure(pnt, typeof(ModInfoFields));
            currentOffset += size;
            var moduleNameSize = findFirstZero(dbiBytes, currentOffset) - currentOffset;
            var moduleName = new String(copySubArray(dbiBytes, currentOffset, moduleNameSize).Select(b => (char)b).ToArray());
            currentOffset += moduleNameSize + 1;
            var objFileNameSize = findFirstZero(dbiBytes, currentOffset) - currentOffset;
            var objFileName = new String(copySubArray(dbiBytes, currentOffset, objFileNameSize).Select(b => (char)b).ToArray());
            currentOffset += objFileNameSize + 1;
            // add padding
            currentOffset += (DWORD - (currentOffset) % DWORD) % DWORD;
            return new DbiModInfoRecord(modInfoFields, moduleName, objFileName);
        }

        // In case that parts are not sequential I created temperary copy of related bytes for a convinience
        private byte[] getStreamParts(StreamName streamName)
        {
            List<byte> bytes = new();
            var pointers = streamName != StreamName.StreamDirectory ? streams[(int)streamName].pointers : pointersOfStreamDirectory;
            var size = streamName != StreamName.StreamDirectory ? streams[(int)streamName].size : sizeOfStreamDirectory;
            for (int i = 0; i < pointers.Length - 1; i++)
            {
                var pointer = pointers[i];
                bytes.AddRange(copySubArray(pointer, blockSize));

            }
            var lastPointer = pointers.Last();
            bytes.AddRange(copySubArray(lastPointer, size - blockSize * (pointers.Length - 1)));
            return bytes.ToArray();
        }

        private void parseDbiStream()
        {
            byte[] dbi = getStreamParts(StreamName.DbiStream);
            parseDbiHeader(dbi.ToArray());
            currentOffset = Marshal.SizeOf(dbiHeader);
            while (currentOffset < dbiHeader.ModInfoSize)
            {
                dbiModInfoRecords.Add(parseDbiModInfoRecord(dbi));
            }
        }

        private void parseStreams()
        {
            parsePdbStream();
            parseDbiStream();
        }

        public void printInfo()
        {
            Console.WriteLine("PDB Info Stream");
            Console.WriteLine(authentity.ToString());
            Console.WriteLine("Debug Info Stream Header");
            Console.WriteLine(dbiHeader.ToString());
            Console.WriteLine("Debug Info Substream");
            for (int i = 0; i < dbiModInfoRecords.Count; i++)
            {
                Console.WriteLine(String.Format("Record {0}", i));
                Console.WriteLine(dbiModInfoRecords[i].ToString());
            }
        }
    }
}