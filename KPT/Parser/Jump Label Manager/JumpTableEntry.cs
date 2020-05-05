using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.Parser.Elements;
using KPT.Parser.Headers;

namespace KPT.Parser.Jump_Label_Manager
{
    /// <summary>
    /// Marks an entry in the games jump table
    /// </summary>
    class JumpTableEntry : IElement
    {

        StCpNumber fileNumber;
        short sequentialChoiceNumber; // suspected to the sequential number of the choice within the file but not 100% guaranteed
        short unknown1;
        short offset; // jump to this offset within the file - note that this is the offset from the end of the header not the start of the file
        short unknown2;
        short unknown3;
        short lookupCode; // the look up code used by IntraFileJump and possibly also InterFileJump
        short unknown4;
        short unknown5;
        DataBox trailingBlock; // this block is usually but is not always entirey 0xFF ands its purpose is not clear

        public JumpTableEntry()
        {
            fileNumber = new StCpNumber();
        }

        public bool Read(BinaryReader br)
        {
            fileNumber.Read(br);
            sequentialChoiceNumber = br.ReadInt16();
            unknown1 = br.ReadInt16();
            offset = br.ReadInt16();
            offset = HandleOffsetOnRead(offset); // since the offset is counted from the end of the header not the start of the file we make our own adjustment to it to turn into a real offset from the start of the file. since we will be dealing with full file streams this makes it easier to reason about and recalcuate the offets.
            unknown2 = br.ReadInt16();
            unknown3 = br.ReadInt16();
            lookupCode = br.ReadInt16();
            unknown4 = br.ReadInt16();
            unknown5 = br.ReadInt16();
            trailingBlock = new DataBox(0x82);
            trailingBlock.Read(br);
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            fileNumber.Write(bw);
            bw.Write(sequentialChoiceNumber);
            bw.Write(unknown1);
            offset = HandleOffsetOnWrite(offset);
            bw.Write(offset);
            bw.Write(unknown2);
            bw.Write(unknown3);
            bw.Write(lookupCode);
            bw.Write(unknown4);
            bw.Write(unknown5);
            trailingBlock.Write(bw);
            return true;
        }

        private short HandleOffsetOnRead(short offset)
        {
            return (short)(offset + StCp_Header.HEADER_SIZE);
        }

        private short HandleOffsetOnWrite(short offset)
        {
           return (short)(offset - StCp_Header.HEADER_SIZE);
        }

        public static string GenerateJumpID(StCpNumber fileNumber, int jumpTargetAddress)
        {
            return fileNumber.ToString() + ":" + jumpTargetAddress.ToString("X");
        }

        public string GetJumpID()
        {
            return GenerateJumpID(fileNumber, offset);
        }

        public void UpdateOffset(short newOffset)
        {
            offset = newOffset;
        }

    }
}
