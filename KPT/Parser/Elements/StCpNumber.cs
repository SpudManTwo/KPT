using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KPT.Parser.Elements
{
    // A block of bytes indicating or referencing a Cp and St number together - appears in headers and jumps
    class StCpNumber : IElement
    {

        short cpNumber;
        short stNumber;

        public bool Read(BinaryReader br)
        {
            cpNumber = br.ReadInt16();
            stNumber = br.ReadInt16();            
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            bw.Write(cpNumber);
            bw.Write(stNumber);
            return true;
        }

        public override string ToString()
        {
            return "St" + stNumber.ToString("X") + "Cp" + cpNumber.ToString("X");
        }

    }
}
