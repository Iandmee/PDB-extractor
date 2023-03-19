using System.Text;

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
        const string RSDS_SIGNATURE = "RSDS";
        int sizeOfCodeViewData;
        int rawDataPointerToCodeView;
        string pathToPDB;
        string guid;


        public PathExtractor(byte[] bytes) : base(bytes)
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

        private void calculatePointerToCodeview()
        {
            var ntHeader = parseInt(NT_HEADER_OFFSET_POINTER);
            var numberOfSections = parseShort(ntHeader + NUMBER_OF_SECTIONS_OFFSET);
            var fileCharacteristic = ntHeader + FILE_CHARACTERISTIC_OFFSET;

            if ((parseShort(fileCharacteristic) & IMAGE_FILE_DEBUG_STRIPPED_FLAG) != 0) // Check existence of the Debug directory

            {
                throw new ArgumentException("No debug file found!");
            }

            var optionalHeaderPointer = ntHeader + OPTIONAL_HEADER_OFFSET;
            var sizeOfOptionalHeader = parseShort(ntHeader + SIZE_OF_OPTIONAL_HEADER_OFFSET);

            var debugDirectoryRvaPointer = optionalHeaderPointer + sizeOfOptionalHeader - SIZE_OF_DATADIRS_AFTER_DEBUG;
            var debugDirectoryRva = parseInt(debugDirectoryRvaPointer);

            var firstSectionHeaderPointer = optionalHeaderPointer + sizeOfOptionalHeader;

            var debugDirectoryPointer = findCorrectSectionForDebugDirectory(firstSectionHeaderPointer, numberOfSections, debugDirectoryRva);
            sizeOfCodeViewData = parseInt(debugDirectoryPointer + SIZE_OF_CODEVIEW_DATA_OFFSET);
            rawDataPointerToCodeView = parseInt(debugDirectoryPointer + RAW_DATA_POINTER_TO_CODEVIEW_OFFSET);
        }

        private int findCorrectSectionForDebugDirectory(int startPos, int numberOfSections, int debugDirectoryRva)
        {
            for (int i = 0; i < numberOfSections; i++)
            {
                var pointerToSection = startPos + SECTION_SIZE * i;
                var virtualAddressOfSection = parseInt(pointerToSection + RVA_VALUE_OF_SECTION_HEADER_OFFSET);
                var sizeOfRawDataOfSection = parseInt(pointerToSection + SIZE_OF_RAW_DATA_OF_SECTION);
                var pointerToRawDataOfSection = parseInt(pointerToSection + POINTER_TO_RAW_DATA_SECTION_HEADER_OFFSET);
                if (debugDirectoryRva >= virtualAddressOfSection && debugDirectoryRva <= virtualAddressOfSection + sizeOfRawDataOfSection)
                {
                    return debugDirectoryRva - virtualAddressOfSection + pointerToRawDataOfSection;
                }
            }
            throw new ArgumentException("No appropriate section found! Debug directory can not be identified!");
        }

        private void extractInfo()
        {
            var signature = System.Text.Encoding.Default.GetString(copySubArray(rawDataPointerToCodeView, QWORD));
            if(signature != RSDS_SIGNATURE)
            {
                throw new ArgumentException("Signature != RSDS");
            }
            guid = parseGUID(rawDataPointerToCodeView + GUID_OFFSET);
            byte[] path = copySubArray(rawDataPointerToCodeView + PDB_PATH_OFFSET,sizeOfCodeViewData - PDB_PATH_OFFSET);
            Array.Resize(ref path, Array.FindLastIndex(path, c => c != 0) + 1);
            pathToPDB = System.Text.Encoding.Default.GetString(path);
        }

        public string getPath()
        {
            return pathToPDB;
        }

        public string getGuid()
        {
            return guid;
        }

    }
}