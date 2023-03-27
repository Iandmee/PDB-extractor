using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdbExtractor
{
    internal class DebugDirectory
    {
        string path;
        string guid;
        public DebugDirectory(string path, string guid)
        {
            this.path = path;
            this.guid = guid;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(String.Format("Pdb file path: {0}", path));
            builder.AppendLine(String.Format("GUID: {0}", guid));
            return builder.ToString();
        }

        public string getPath()
        {
            return path;
        }
    }
}
