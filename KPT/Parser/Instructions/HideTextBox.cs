using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using KPT.Parser.Elements;

namespace KPT.Parser.Instructions
{
    class HideTextBox : IInstruction
    {

        Opcode opcode;
        int fadeType; // really a bool - zero for standard disappearance and non-zero for a fade down

        public bool Read(BinaryReader br)
        {
            opcode = FileIOHelper.ReadOpcode(br);
            fadeType = br.ReadUInt16();
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            bw.Write((short)opcode);
            bw.Write((short)fadeType);
            return true;
        }

    }
}
