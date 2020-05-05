using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KPT.Parser.Instructions
{
    class IntraFileJump : IInstruction
    {
        // make a jump within the same file - appears to use a lookup table in St000_SldtDat.bin
        Opcode opcode;
        // one of these shorts is a look up code and the other is unknown. sometimes they are the same and sometimes they are different. so there is some currently unknown logic that may affect branching in an unpredictable way.
        short firstLookUpCode;
        short secondLookUpCode;

        public bool Read(BinaryReader br)
        {
            opcode = FileIOHelper.ReadOpcode(br);
            firstLookUpCode = br.ReadInt16();
            secondLookUpCode = br.ReadInt16();
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            bw.Write((short)opcode);
            bw.Write(firstLookUpCode);
            bw.Write(secondLookUpCode);
            return true;
        }
    }
}
