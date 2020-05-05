using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.Parser.Elements;
using KPT.Parser.Headers;
using KPT.Parser.Footers;

namespace KPT.Parser.Jump_Label_Manager
{
    /// <summary>
    /// Read and write the games jump table
    /// </summary>
    /// <remarks>
    /// The game handles jumps by looking up indexes in a jump table that is stored as a file on disk. 
    /// </remarks>
    class JumpTableInterface
    {
        private static readonly string JUMP_TABLE_FILE = Path.Combine("PSP_GAME", "USRDIR", "St000", "St000", "St000_SldtDat.bin");

        St_Header header;
        List<JumpTableEntry> jumpTableEntries;
        St_Footer footer;

        public JumpTableInterface()
        {
            header = new St_Header();
            footer = new St_Footer();
            jumpTableEntries = new List<JumpTableEntry>();
        }

        public void LoadJumpTable()
        {
            string originalFilePath = Path.Combine(ProjectFolder.rootDir, ProjectFolder.unpackedGameFilesDir, JUMP_TABLE_FILE);            

            FileStream fs = new FileStream(originalFilePath, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            footer.Read(br);
            header.Read(br);

            long streamEnd = br.BaseStream.Length - ElementHelper.GetElementSize(footer);

            while (br.BaseStream.Position != streamEnd)
            {
                var nextEntry = new JumpTableEntry();
                nextEntry.Read(br);
                jumpTableEntries.Add(nextEntry);
            }

            br.Close();
        }

        public void SaveJumpTable()
        {
            string targetFilePath = Path.Combine(ProjectFolder.rootDir, ProjectFolder.reassembledGameFilesDir, JUMP_TABLE_FILE);

            FileStream fs = new FileStream(targetFilePath, FileMode.Open);
            BinaryWriter bw = new BinaryWriter(fs);

            header.Write(bw);

            foreach (var entry in jumpTableEntries)
            {
                entry.Write(bw);
            }

            footer.Write(bw);

            bw.Close();

        }

        public List<JumpTableEntry> GetJumpTableEntries()
        {
            return jumpTableEntries;
        }

    }
}
