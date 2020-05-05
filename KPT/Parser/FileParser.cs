using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.Parser.Elements;
using KPT.Parser.Instructions;
using KPT.Parser.Headers;
using KPT.Parser.Footers;
using KPT.Parser.Elements;
using KPT.Parser.Jump_Label_Manager;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace KPT.Parser
{
    /// <summary>
    /// Represents a processed file
    /// </summary>
    class KCFile
    {
        public IHeader header;
        public List<IInstruction> instructions;
        public IFooter footer;
    }
    // perhaps FileParser and KCFile should be merged?

    class FileParser
    {

        public static bool IsParseable(string fileName)
        {
            Regex regex = new Regex(@"St[0-9A-F]{3}_Cp[0-9A-F]{4}\.bin"); // search for the StXXX_CpXXXX.bin format. since that's the only file we can parse at the moment there's not much detail to go into to determine whether or not it is parseable.
            Match match = regex.Match(fileName);
            return match.Success;
        }

        public FileParser()
        {

        }

        /// <summary>
        /// Takes a file and breaks it down into IElements then returns it in a structured format
        /// </summary>
        /// <param name="br">The file to process</param>
        /// <param name="fileName">The name of the file to be processed (used only for displaying error messages)</param>
        /// <param name="jumpLabelManager">The jump label manager to use when processing file (pass null to disable jump tracking)</param>
        /// <returns>A class containing the processed file data</returns>
        public KCFile ParseFile(BinaryReader br, string fileName, JumpLabelManager jumpLabelManager)
        {
            KCFile workingFile = new KCFile();
            List<IInstruction> instructions = new List<IInstruction>();

            workingFile.footer = ReadFooter(br);
            workingFile.header = ReadHeader(br);

            StCpNumber fileNumber = (workingFile.header as StCp_Header).GetFileNumber(); // more than anything else this basically cements that this function reads only StCp files which should really be clarified at some point

            long streamEnd = br.BaseStream.Length - ElementHelper.GetElementSize(workingFile.footer);

            while (br.BaseStream.Position != streamEnd) // will need to check this for accuracy as it has been unreliable in some cases in the past
            {
                Opcode opcode = FileIOHelper.ReadOpcode(br);
                br.BaseStream.Position -= OpcodeInfo.OPCODE_SIZE; // set the position back by 2, the size of the opcodes, as the instruction parsers will expect to given a position starting with an opcode

                if (opcode == Opcode.INVALID)
                {
                    string errorMessage = string.Format("There was an unexpected opcode when reading file {0} at position {1} after reading {2} instructions", fileName, br.BaseStream.Position.ToString("X"), instructions.Count.ToString());
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                if (jumpLabelManager != null) // we want to support running this without a jumplabelmanager just in case
                {

                    long currentAddress = br.BaseStream.Position;

                    if (jumpLabelManager.IsJumpTarget(fileNumber, (int)currentAddress))
                    {
                        var virtualLabel = jumpLabelManager.CreateVirtualLabel(fileNumber, (int)currentAddress);
                        instructions.Add(virtualLabel);
                    }

                }

                IInstruction newInstruction;
                Type instructionParserType = OpcodeInfo.GetInstructionParserType(opcode);
                
                if (instructionParserType == typeof(InstructionBox))
                {
                    newInstruction = ElementHelper.MakeInstructionBox(opcode);
                    newInstruction.Read(br);
                }
                else
                {
                    newInstruction = (IInstruction)Activator.CreateInstance(instructionParserType);
                    newInstruction.Read(br);
                }

                instructions.Add(newInstruction);

            }

            //string finishMessage = string.Format("Parsed file {0} with {1} instructions", fileName, instructions.Count.ToString());
            //MessageBox.Show(finishMessage);

            workingFile.instructions = instructions;
            return workingFile;
            
        }

        public void WriteFile(KCFile file, string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            file.header.Write(bw);

            foreach (var instruction in file.instructions)
            {
                instruction.Write(bw);
            }

            file.footer.Write(bw);

            bw.Close();
            fs.Close();
        }

        private IHeader ReadHeader(BinaryReader br)
        {
            StCp_Header header = new StCp_Header();
            if (!header.Read(br))
            {
                string errorMessage = "Failed to read header of file {0}. Corrupt or invalid header?";
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            return header;
        }

        private IFooter ReadFooter(BinaryReader br)
        {
            St_Footer footer = new St_Footer();

            footer.Read(br);

            return footer;
        }

    }
}
