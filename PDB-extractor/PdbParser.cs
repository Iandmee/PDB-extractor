using PDB_extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

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
        const int NUMBER_OF_PAGES_OFFSET = 0x28;
        const int SIZE_OF_STREAM_DIRECTORY_OFFSET = 0x2c;
        const int POINTER_TO_STREAM_DIRECTORY_OFFSET = 0x34;
        const string ACCEPTED_SIGNATURE = "Microsoft C/C++ MSF 7.00";
        int blockSize;
        int numberOfPages;
        int sizeOfStreamDirectory;
        int pointerToStreamDirectory;
        int numberOfStreamDirectoryParts;
        int[] pointersOfStreamDirectory;
        int currentOffset;
        List<Stream> streams;
        Authentity authentity;
        List<DbiModInfoRecord> dbiModInfoRecords;
        DbiStreamHeader dbiHeader;

        public PdbParser(byte[] bytes) : base(bytes)
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
            var signature = System.Text.Encoding.Default.GetString(copySubArray(0, 24));
            if (signature != ACCEPTED_SIGNATURE)
            {
                throw new ArgumentException("Wrong Pdb file format!");
            }
        }

        private void getStreamDirectoryPointers()
        {
            blockSize = parseShort(BLOCK_SIZE_OFFSET);
            numberOfPages = parseInt(NUMBER_OF_PAGES_OFFSET);
            sizeOfStreamDirectory = parseInt(SIZE_OF_STREAM_DIRECTORY_OFFSET);
            pointerToStreamDirectory = blockSize * parseInt(POINTER_TO_STREAM_DIRECTORY_OFFSET);
            numberOfStreamDirectoryParts = sizeOfStreamDirectory % blockSize == 0 ? sizeOfStreamDirectory / blockSize : sizeOfStreamDirectory / blockSize + 1;
            pointersOfStreamDirectory = new int[numberOfStreamDirectoryParts];
            for (int i = 0; i < numberOfStreamDirectoryParts; i++)
            {
                pointersOfStreamDirectory[i] = parseInt(pointerToStreamDirectory + i * QWORD) * blockSize;
            }
        }

        private void parseStreamDirectory()
        {
            var bytes = getStreamParts(StreamName.StreamDirectory);
            var numberOfStreams = parseInt(bytes, currentOffset);
            currentOffset += QWORD;
            for (int i = 0; i < numberOfStreams; i++)
            {
                var streamSize = parseInt(bytes, currentOffset);
                streams.Add(new Stream(streamSize, blockSize));
                currentOffset += QWORD;
            }
            for (int i = 0; i < streams.Count; i++)
            {
                if (streams[i].size == 0)
                {
                    continue;
                }
                for (int pointer = 0; pointer < streams[i].pointers.Length; pointer++)
                {
                    streams[i].pointers[pointer] = parseInt(bytes, currentOffset) * blockSize;
                    currentOffset += QWORD;
                }
            }
            currentOffset = 0;
        }

        private void parseAuthority(byte[] bytes)
        {
            var version = (PdbStreamVersion)parseInt(bytes, currentOffset);
            currentOffset += 2 * QWORD;
            var age = parseInt(bytes, currentOffset);
            currentOffset += QWORD;
            StringBuilder guid = new StringBuilder();
            guid.Append(reversedHexSubArray(bytes, currentOffset, QWORD)).Append("-");
            currentOffset += QWORD;
            guid.Append(reversedHexSubArray(bytes, currentOffset, DWORD)).Append("-");
            currentOffset += DWORD;
            guid.Append(reversedHexSubArray(bytes, currentOffset, DWORD)).Append("-");
            currentOffset += DWORD;
            guid.Append(hexSubArray(bytes, currentOffset, QWORD * 2));
            currentOffset += QWORD * 2;
            authentity = new Authentity(version.ToString(), guid.ToString(), age);
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
            ModInfoFields modInfoFields = new ModInfoFields();
            var size = Marshal.SizeOf(modInfoFields);
            IntPtr pnt = Marshal.AllocHGlobal(size);
            Marshal.Copy(dbiBytes, currentOffset, pnt, size);
            modInfoFields = (ModInfoFields)Marshal.PtrToStructure(pnt, typeof(ModInfoFields));
            currentOffset += size;
            var moduleNameSize = findFirstZero(dbiBytes, currentOffset) - currentOffset;
            string moduleName = System.Text.Encoding.Default.GetString(copySubArray(dbiBytes, currentOffset, moduleNameSize));
            currentOffset += moduleNameSize + 1;
            var objFileNameSize = findFirstZero(dbiBytes, currentOffset) - currentOffset;
            string objFileName = System.Text.Encoding.Default.GetString(copySubArray(dbiBytes, currentOffset, objFileNameSize));
            currentOffset += objFileNameSize + 1;
            // add padding
            currentOffset += (QWORD - (currentOffset) % QWORD) % QWORD;
            return new DbiModInfoRecord(modInfoFields, moduleName, objFileName);
        }

        // In case that parts are not sequential I created temperary copy of related bytes for a convinience
        private byte[] getStreamParts(StreamName streamName)
        {
            List<byte> bytes = new List<byte>();
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