
namespace PdbExtractor
{
    class PathExtractor : PdbExtractor.BytesWrapper
    {
        const int NT_HEADER_OFFSET_POINTER = 0x3C;
        const int IMAGE_FILE_DEBUG_STRIPPED_FLAG = 0x0200;
        const int FILE_CHARACTERISTIC_OFFSET = 0x16;
        const int OPTIONAL_HEADER_OFFSET = 0x18;
        const int SIZE_OF_OPTIONAL_HEADER_OFFSET = 0x14;
        const int RVA_VALUE_OF_SECTION_HEADER_OFFSET = 0xC;
        const int SIZE_OF_RAW_DATA_OF_SECTION = 0x10;
        const int POINTER_TO_RAW_DATA_SECTION_HEADER_OFFSET = 0x14;
        const int SIZE_OF_CODEVIEW_DATA_OFFSET = 0x10;
        const int RAW_DATA_POINTER_TO_CODEVIEW_OFFSET = 0x18;
        const int PDB_PATH_OFFSET = 0x18;
        const int NUMBER_OF_SECTIONS_OFFSET = 0x6;
        const int SIZE_OF_DATADIRS_AFTER_DEBUG = 0x8 * 10;
        const int SECTION_SIZE = 0x28;
        const int GUID_OFFSET = 0x4;
        const int SIZE_OF_DEBUG_DIRECTORIES_OFFSET = 0x4;
        const int SIZE_OF_DEBUG_DIRECTORY = 0x1C;
        const int DEBUG_DIRECTORY_TYPE_OFFSET = 0xC;
        const int CODEVIEW_DIRECTORY_TYPE = 0x2;
        const string RSDS_SIGNATURE = "RSDS";
        const string MZ_SIGNATURE = "MZ";
        const string PE_SIGNATURE = "PE\0\0";
        int sizeOfDebugDirectories;
        int firstDebugDirectoryPointer;
        DebugDirectory[] directories;

        public PathExtractor(FileStream fileStream) : base(fileStream)
        {
            try
            {
                calculatePointerToCodeview();
                extractInfo();
            }
            catch
            {
                Console.Error.WriteLine("Some error occured! (probably wrong file format)");
                throw;
            }
        }

        private void checkPESignatures(int ntHeader)
        {
            String mzSignature = new String(copySubArray(0, WORD).Select(b => (char)b).ToArray());
            if (mzSignature != MZ_SIGNATURE)
            {
                throw new ArgumentException("No MZ signature found! (probably it is not a PE file)");
            }
            String peSignature = new String(copySubArray(ntHeader, DWORD).Select(b => (char)b).ToArray());
            if (peSignature != PE_SIGNATURE)
            {
                throw new ArgumentException("No PE signature found! (probably it is not a PE file)");
            }
        }

        private void calculatePointerToCodeview()
        {
            var ntHeader = parseInt(NT_HEADER_OFFSET_POINTER);
            checkPESignatures(ntHeader);
            // get required values by knowing ntHeader address
            var numberOfSections = parseShort(ntHeader + NUMBER_OF_SECTIONS_OFFSET);
            var optionalHeaderPointer = ntHeader + OPTIONAL_HEADER_OFFSET;
            var sizeOfOptionalHeader = parseShort(ntHeader + SIZE_OF_OPTIONAL_HEADER_OFFSET);
            var fileCharacteristic = ntHeader + FILE_CHARACTERISTIC_OFFSET;
            // check existence of the Debug directory
            if ((parseShort(fileCharacteristic) & IMAGE_FILE_DEBUG_STRIPPED_FLAG) != 0)
            {
                throw new ArgumentException("No debug file found!");
            }
            // calculate debug directory RVA by knowing optionalHeader parameters
            var firstDebugDirectoryRvaPointer = optionalHeaderPointer + sizeOfOptionalHeader - SIZE_OF_DATADIRS_AFTER_DEBUG;
            var firstDebugDirectoryRva = parseInt(firstDebugDirectoryRvaPointer);
            // get a pointer to the first section header after optional header for the next iterating through the sections headers
            var firstSectionHeaderPointer = optionalHeaderPointer + sizeOfOptionalHeader;
            // find a section, where Debug directory is located by iterating through the sections and calculating RVA boundaries
            firstDebugDirectoryPointer = findCorrectSectionForDebugDirectory(firstSectionHeaderPointer, numberOfSections, firstDebugDirectoryRva);
            sizeOfDebugDirectories = parseInt(firstDebugDirectoryRvaPointer + SIZE_OF_DEBUG_DIRECTORIES_OFFSET);
        }

        private int findCorrectSectionForDebugDirectory(int startPos, int numberOfSections, int debugDirectoryRva)
        {
            for (int i = 0; i < numberOfSections; i++)
            {
                var pointerToSection = startPos + SECTION_SIZE * i;
                var virtualAddressOfSection = parseInt(pointerToSection + RVA_VALUE_OF_SECTION_HEADER_OFFSET);
                var sizeOfDataOfSection = parseInt(pointerToSection + SIZE_OF_RAW_DATA_OF_SECTION);
                var pointerToRawDataOfSection = parseInt(pointerToSection + POINTER_TO_RAW_DATA_SECTION_HEADER_OFFSET);
                if (debugDirectoryRva >= virtualAddressOfSection && debugDirectoryRva < virtualAddressOfSection + sizeOfDataOfSection)
                {
                    return debugDirectoryRva - virtualAddressOfSection + pointerToRawDataOfSection;
                }
            }
            throw new ArgumentException("No appropriate section found! Debug directory can not be identified!");
        }

        private void extractInfo()
        {
            if (sizeOfDebugDirectories == 0)
            {
                throw new ArgumentException("Debug directory(s) size 0!");
            }
            if (sizeOfDebugDirectories % SIZE_OF_DEBUG_DIRECTORY != 0)
            {
                throw new ArgumentException("Wrong debug directory(s) size!");
            }
            int debugDirectoryPointer;
            int sizeOfCodeViewData;
            int rawDataPointerToCodeView;
            string signature;
            int directoryType;
            int numberOfDirectories = sizeOfDebugDirectories / SIZE_OF_DEBUG_DIRECTORY;
            List<DebugDirectory> directoriesList = new List<DebugDirectory>();
            for (int i = 0; i < numberOfDirectories; i++)
            {
                debugDirectoryPointer = firstDebugDirectoryPointer + i * SIZE_OF_DEBUG_DIRECTORY;
                directoryType = parseInt(debugDirectoryPointer + DEBUG_DIRECTORY_TYPE_OFFSET);
                if (directoryType != CODEVIEW_DIRECTORY_TYPE)
                {
                    continue;
                }
                sizeOfCodeViewData = parseInt(debugDirectoryPointer + SIZE_OF_CODEVIEW_DATA_OFFSET);
                rawDataPointerToCodeView = parseInt(debugDirectoryPointer + RAW_DATA_POINTER_TO_CODEVIEW_OFFSET);
                signature = new String(copySubArray(rawDataPointerToCodeView, DWORD).Select(b => (char)b).ToArray());
                if (signature != RSDS_SIGNATURE)
                {
                    //throw new ArgumentException("Signature != RSDS in some CodeView directory");
                    // in case, if you want just to skip "invalid" codeview directories you should keep it like this. In other cases uncomment the previous line, and comment next one.
                    continue;
                }
                byte[] pathBytes = copySubArray(rawDataPointerToCodeView + PDB_PATH_OFFSET, sizeOfCodeViewData - PDB_PATH_OFFSET);
                Array.Resize(ref pathBytes, Array.FindLastIndex(pathBytes, c => c != 0) + 1);
                string pathString = new String(pathBytes.Select(b => (char)b).ToArray());
                string guid = parseGUID(rawDataPointerToCodeView + GUID_OFFSET);
                directoriesList.Add(new DebugDirectory(pathString, guid));
            }
            directories = directoriesList.ToArray();
        }

        public DebugDirectory[] getDirectories()
        {
            return directories;
        }

    }
}