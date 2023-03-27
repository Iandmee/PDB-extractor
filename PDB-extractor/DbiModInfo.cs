using System.Text;

namespace PdbExtractor
{
    class DbiModInfoRecord
    {
        ModInfoFields fields;
        readonly string moduleName;
        readonly string objFileName;
        public DbiModInfoRecord(ModInfoFields fields, string moduleName, string objFileName)
        {
            this.fields = fields;
            this.moduleName = moduleName;
            this.objFileName = objFileName;
        }
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine(fields.ToString());
            builder.AppendLine(String.Format("  ModuleName: {0}", moduleName));
            builder.AppendLine(String.Format("  ObjFileName: {0}", objFileName));
            return builder.ToString();
        }
    }
}
