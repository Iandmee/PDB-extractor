using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdbExtractor
{
    enum PdbStreamVersion : uint
    {
        VC2 = 19941610,
        VC4 = 19950623,
        VC41 = 19950814,
        VC50 = 19960307,
        VC98 = 19970604,
        VC70Dep = 19990604,
        VC70 = 20000404,
        VC80 = 20030901,
        VC110 = 20091201,
        VC140 = 20140508,
    };

    // this is a class, because I have to modify guid from raw bytes, and I can not just map it. 
    class Authentity
    {
        string StreamVersion;
        string Guid;
        int Age;

        public Authentity(string version, string guid, int age)
        {
            this.StreamVersion = version;
            this.Guid = guid;
            this.Age = age;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(String.Format("StreamVersion: {0}", StreamVersion));
            builder.AppendLine();
            builder.AppendLine("Authentity");
            builder.AppendLine(String.Format("Age: 0x{0}", Convert.ToString(Age,16)));
            builder.AppendLine(String.Format("GUID: {0}", Guid));
            return builder.ToString();
        }
    }
}
