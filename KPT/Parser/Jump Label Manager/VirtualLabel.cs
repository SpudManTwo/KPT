using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.Parser.Elements;

namespace KPT.Parser.Jump_Label_Manager
{
    /// <summary>
    /// A fake instruction representing the target of a jump
    /// </summary>
    /// <remarks>
    /// Slotted into instruction lists in front of instructions that are targets of jumps. Allows the jump table to updated as Write calls are made.
    /// </remarks>
    class VirtualLabel : IInstruction
    {
        int currentAddress;
        JumpTableEntry pairedEntry;

        public VirtualLabel(JumpTableEntry toPair)
        {
            pairedEntry = toPair;
        }

        public bool Read(BinaryReader br)
        {
            currentAddress = (int)br.BaseStream.Position;
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            currentAddress = (int)bw.BaseStream.Position;
            pairedEntry.UpdateOffset((short)currentAddress);
            return true;
        }

    }
}
