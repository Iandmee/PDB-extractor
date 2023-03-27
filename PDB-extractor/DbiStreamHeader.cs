using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PdbExtractor
{
    enum DbiStreamVersion : uint
    {
        VC41 = 930803,
        V50 = 19960307,
        V60 = 19970606,
        V70 = 19990903,
        V110 = 20091201
    };

    struct DbiStreamHeader
    {
        public int VersionSignature;
        public DbiStreamVersion VersionHeader;
        public uint Age;
        public ushort GlobalStreamIndex;
        public ushort BuildNumber;
        public ushort PublicStreamIndex;
        public ushort PdbDllVersion;
        public ushort SymRecordStream;
        public ushort PdbDllRbld;
        public int ModInfoSize;
        public int SectionContributionSize;
        public int SectionMapSize;
        public int SourceInfoSize;
        public int TypeServerMapSize;
        public uint MFCTypeServerIndex;
        public int OptionalDbgHeaderSize;
        public int ECSubstreamSize;
        public ushort Flags;
        public ushort Machine;
        public uint Padding;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(String.Format("  VersionSignature: 0x{0}", Convert.ToString(VersionSignature, 16)));
            builder.AppendLine(String.Format("  VersionHeader: {0}", VersionHeader));
            builder.AppendLine(String.Format("  Age: 0x{0}", Convert.ToString(Age, 16)));
            builder.AppendLine(String.Format("  GlobalStreamIndex: 0x{0}", Convert.ToString(GlobalStreamIndex, 16)));
            builder.AppendLine(String.Format("  BuildNumber: 0x{0}", Convert.ToString(BuildNumber, 16)));
            builder.AppendLine(String.Format("  PublicStreamIndex: 0x{0}", Convert.ToString(PublicStreamIndex, 16)));
            builder.AppendLine(String.Format("  PdbDllVersion: 0x{0}", Convert.ToString(PdbDllVersion, 16)));
            builder.AppendLine(String.Format("  SymRecordStream: 0x{0}", Convert.ToString(SymRecordStream, 16)));
            builder.AppendLine(String.Format("  PdbDllRbld: 0x{0}", Convert.ToString(PdbDllRbld, 16)));
            builder.AppendLine(String.Format("  ModInfoSize: 0x{0}", Convert.ToString(ModInfoSize, 16)));
            builder.AppendLine(String.Format("  SectionContributionSize: 0x{0}", Convert.ToString(SectionContributionSize, 16)));
            builder.AppendLine(String.Format("  SectionMapSize: 0x{0}", Convert.ToString(SectionMapSize, 16)));
            builder.AppendLine(String.Format("  SourceInfoSize: 0x{0}", Convert.ToString(SourceInfoSize, 16)));
            builder.AppendLine(String.Format("  TypeServerMapSize: 0x{0}", Convert.ToString(TypeServerMapSize, 16)));
            builder.AppendLine(String.Format("  MFCTypeServerIndex: 0x{0}", Convert.ToString(MFCTypeServerIndex, 16)));
            builder.AppendLine(String.Format("  OptionalDbgHeaderSize: 0x{0}", Convert.ToString(OptionalDbgHeaderSize, 16)));
            builder.AppendLine(String.Format("  ECSubstreamSize: 0x{0}", Convert.ToString(ECSubstreamSize, 16)));
            builder.AppendLine(String.Format("  Flags: 0x{0}", Convert.ToString(Flags, 16)));
            builder.AppendLine(String.Format("  Machine: 0x{0}", Convert.ToString(Machine, 16)));
            return builder.ToString();
        }
    }
}


