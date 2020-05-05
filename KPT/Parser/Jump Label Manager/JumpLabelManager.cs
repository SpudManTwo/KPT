using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPT.Parser.Elements;

namespace KPT.Parser.Jump_Label_Manager
{

    /// <summary>
    /// Class to help manage jump labels
    /// </summary>
    class JumpLabelManager
    {
        /// <summary>
        /// Map jump ids to entries in the jump table
        /// </summary>
        private Dictionary<string, JumpTableEntry> jumpLabelMap;

        public JumpLabelManager(List<JumpTableEntry> jumpTableEntries)
        {

            jumpLabelMap = new Dictionary<string, JumpTableEntry>();

            foreach (var entry in jumpTableEntries)
            {
                string jumpID = entry.GetJumpID();
                jumpLabelMap[jumpID] = entry;
            }
        
        }

        public bool IsJumpTarget(StCpNumber fileNumber, int address)
        {
            var tempLabel = JumpTableEntry.GenerateJumpID(fileNumber, address);
            return jumpLabelMap.Keys.Contains(tempLabel);
        }

        public VirtualLabel CreateVirtualLabel(StCpNumber fileNumber, int address)
        {
            var jumpLabel = JumpTableEntry.GenerateJumpID(fileNumber, address);
            var jumpTableEntry = jumpLabelMap[jumpLabel];
            var virtualLabel = new VirtualLabel(jumpTableEntry);
            return virtualLabel;
        }

    }
}
