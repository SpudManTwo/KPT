using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.Parser.Elements;

namespace KPT.Parser.Instructions
{
    class InterFileJump : IInstruction
    {
        // make a jump to a different file - possibly also uses lookup table in St000_SldtDat.bin
        Opcode opcode;
        StCpNumber fileNumber;
        // same deal as the look up codes in InterFileJump
        short firstLookUpCode;
        short secondLookUpCode;

        public InterFileJump()
        {
            fileNumber = new StCpNumber();
        }

        public bool Read(BinaryReader br)
        {
            opcode = FileIOHelper.ReadOpcode(br);
            fileNumber.Read(br);
            firstLookUpCode = br.ReadInt16();
            secondLookUpCode = br.ReadInt16();
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            bw.Write((short)opcode);
            fileNumber.Write(bw);
            bw.Write(firstLookUpCode);
            bw.Write(secondLookUpCode);
            return true;
        }
    }
}