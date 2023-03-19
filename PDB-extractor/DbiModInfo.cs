using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PDB_extractor
{

    class DbiModInfoRecord
    {
        ModInfoFields fields;
        string moduleName;
        string objFileName;
        public DbiModInfoRecord(ModInfoFields fields, string moduleName, string objFileName)
        {
            this.fields = fields;
            this.moduleName = moduleName;
            this.objFileName = objFileName;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(fields.ToString());
            builder.AppendLine(String.Format("  ModuleName: {0}", moduleName));
            builder.AppendLine(String.Format("  ObjFileName: {0}", objFileName));
            return builder.ToString();
        }
    }
}
