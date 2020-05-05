using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KPT.Parser.Headers
{
    /// <summary>
    /// A more generic header used by other members of the St000 family - its purpose is entirely unknown
    /// </summary>
    class St_Header : IHeader
    {
        short unknown;

        public bool Read(BinaryReader br)
        {
            unknown = br.ReadInt16();
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            bw.Write(unknown);
            return true;
        }
    }

}
