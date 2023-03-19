using System.Text;
namespace PDB_extractor
{
    struct ModInfoFields
    {
        public uint Unused1;
        public ushort Section;
        public ushort Padding1;
        public int Offset;
        public int Size;
        public uint Characteristics;
        public ushort ModuleIndex;
        public ushort Padding2;
        public uint DataCrc;
        public uint RelocCrc;
        public ushort Flags;
        public ushort ModuleSymStream;
        public uint SymByteSize;
        public uint C11ByteSize;
        public uint C13ByteSize;
        public ushort SourceFileCount;
        public ushort Padding;
        public uint Unused2;
        public uint SourceFileNameIndex;
        public uint PdbFilePathNameIndex;

        override public string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine(String.Format("  Section: 0x{0}", Convert.ToString(Section, 16)));
            builder.AppendLine(String.Format("  Offset: 0x{0}", Convert.ToString(Offset, 16)));
            builder.AppendLine(String.Format("  Size: 0x{0}", Convert.ToString(Size, 16)));
            builder.AppendLine(String.Format("  Characteristics: 0x{0}", Convert.ToString(Characteristics, 16)));
            builder.AppendLine(String.Format("  ModuleIndex: 0x{0}", Convert.ToString(ModuleIndex, 16)));
            builder.AppendLine(String.Format("  DataCrc: 0x{0}", Convert.ToString(DataCrc, 16)));
            builder.AppendLine(String.Format("  RelocCrc: 0x{0}", Convert.ToString(RelocCrc, 16)));
            builder.AppendLine(String.Format("  Flags: 0x{0}", Convert.ToString(Flags, 16)));
            builder.AppendLine(String.Format("  ModuleSymStream: 0x{0}", Convert.ToString(ModuleSymStream, 16)));
            builder.AppendLine(String.Format("  SymByteSize: 0x{0}", Convert.ToString(SymByteSize, 16)));
            builder.AppendLine(String.Format("  C11ByteSize: 0x{0}", Convert.ToString(C11ByteSize, 16)));
            builder.AppendLine(String.Format("  C13ByteSize: 0x{0}", Convert.ToString(C13ByteSize, 16)));
            builder.AppendLine(String.Format("  SourceFileCount: 0x{0}", Convert.ToString(SourceFileCount, 16)));
            builder.AppendLine(String.Format("  SourceFileNameIndex: 0x{0}", Convert.ToString(SourceFileNameIndex, 16)));
            builder.AppendLine(String.Format("  PdbFilePathNameIndex: 0x{0}", Convert.ToString(PdbFilePathNameIndex, 16)));
            return builder.ToString();
        }
    }
}
