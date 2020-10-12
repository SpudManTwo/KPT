using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPT.Parser.Instructions;
using System.Windows.Forms;

namespace KPT.Parser
{  

    /// <summary>
    /// A list of opcodes known to the program
    /// </summary>
    enum Opcode
    {
        INVALID = -1,
        EOF = -2,

        U_003 = 0x003,
        U_008 = 0x008,
        U_00C = 0x00C,
        U_00F = 0x00F,
        U_01E = 0x01E,
        U_0B4 = 0x0B4,
        U_03C = 0x03C,

        U_100 = 0x100,
        U_101 = 0x101,
        U_111 = 0x111, // purpose unclear - seems to almost exclusively (exclusively?) appear after jumps have been made but does not appear to actually function as a jump label
        INTRA_FILE_JUMP = 0x112,
        INTER_FILE_JUMP = 0x113,
        U_120 = 0x120,
        U_121 = 0x121,
        U_130 = 0x130,
        U_131 = 0x131,
        U_132 = 0x132,
        U_160 = 0x160,
        CHANGE_MUSIC = 0x170,
        U_171 = 0x171,
        U_173 = 0x173,
        U_174 = 0x174,
        U_182 = 0x182,
        U_190 = 0x190,
        U_191 = 0x191,
        U_192 = 0x192,
        U_1A0 = 0x1A0,
        U_1C0 = 0x1C0,

        U_205 = 0x205,
        U_210 = 0x210,
        U_211 = 0x211,
        U_214 = 0x214,
        
        U_302 = 0x302,
        U_303 = 0x303,
        U_30B = 0x30B,
        U_30C = 0x30C,

        U_401 = 0x401,
        WAIT = 0x402, // delay the execution of the next instruction for a few seconds
        U_403 = 0x403,
        ROTATE_CAMERA = 0x404, 
        U_405 = 0x405,
        U_406 = 0x406,
        U_407 = 0x407,

        BASIC_TEXT_BOX = 0x500,
        U_501 = 0x501,
        U_502 = 0x502,
        HIDE_TEXT_BOX = 0x504,

        SHOW_IMAGE = 0x600,
        U_601 = 0x601,
        U_602 = 0x602,
        U_603 = 0x603,

        CHOICE_DIALOG = 0x701,

        PLAY_INTERRUPT = 0x900, // used to play the video files with the moving text and image for dramatic scenes 

        U_1000 = 0x1000,
        LOCATION_CARD = 0x1001,
        
    }


    static class OpcodeInfo
    {

        public static int OPCODE_SIZE = 2;

        /// <summary>
        /// A dictionary of instructions and their sizes. Size is defined as the combined size of the opcode and any arguments it takes. Mainly used for Boxes.
        /// </summary>
        static Dictionary<Opcode, int> instructionSize = new Dictionary<Opcode, int>
        {
            { Opcode.U_003, 2 }, // educated guess, extremely unconfirmed
            { Opcode.U_008, 2 }, // educated guess
            { Opcode.U_00C, 2 },
            { Opcode.U_00F, 2 },
            { Opcode.U_01E, 2 }, // educated guess
            { Opcode.U_0B4, 2 }, // eye balled so not 100% confirmed
            { Opcode.U_03C, 2 },

            { Opcode.U_100, 2 }, // educated guess
            { Opcode.U_101, 4 }, // i think this is correct but the fact it seems to be such an extreme instruction (see comments on this opcode in instruction parser map) makes it hard to see
            { Opcode.U_111, 4 }, // educated guess
            { Opcode.U_120, 4 },
            { Opcode.U_121, 6 }, // educated guess, extremely unconfirmed
            { Opcode.U_130, 4 },
            { Opcode.U_131, 4 },
            { Opcode.U_132, 2 },
            { Opcode.U_160, 2 },
            { Opcode.CHANGE_MUSIC, 4 }, // short opcode, byte songChoice, byte unknown
            { Opcode.U_171, 4 },
            { Opcode.U_173, 4 },
            { Opcode.U_174, 2 },
            { Opcode.U_182, 10 },
            { Opcode.U_190, 4 }, // educated guess
            { Opcode.U_191, 6 },
            { Opcode.U_192, 4 },
            { Opcode.U_1A0, 6 },
            { Opcode.U_1C0, 2 },

            { Opcode.U_205, 4 },
            { Opcode.U_210, 4 },
            { Opcode.U_211, 4 },
            { Opcode.U_214, 4 },

            { Opcode.U_302, 2 },
            { Opcode.U_303, 2 },
            { Opcode.U_30B, 2 },
            { Opcode.U_30C, 2 }, // only had one example of this so far

            { Opcode.U_401, 4 },
            { Opcode.WAIT, 2 },
            { Opcode.U_403, 2 },
            { Opcode.ROTATE_CAMERA, 7 }, // short opcode, bool directionOfRotation, short extentOfRotation, short speedOfRotation
            { Opcode.U_405, 4 },
            { Opcode.U_406, 4 }, // i should be more confident of this than i actually am
            { Opcode.U_407, 2 },

            { Opcode.U_601, 8 },
            { Opcode.U_602, 8 }, // eye balled so not 100% confirmed
            { Opcode.U_603, 2 },

            { Opcode.PLAY_INTERRUPT, 6 }, // short opcode, short videofile, short unknown

            { Opcode.U_1000, 2 },
        };

        static Dictionary<Opcode, Type> instructionParserMap = new Dictionary<Opcode, Type>
        {
            { Opcode.U_003, typeof(InstructionBox) },
            { Opcode.U_008, typeof(InstructionBox) },
            { Opcode.U_00C, typeof(InstructionBox) },
            { Opcode.U_00F, typeof(InstructionBox) },
            { Opcode.U_01E, typeof(InstructionBox) },
            { Opcode.U_0B4, typeof(InstructionBox) },
            { Opcode.U_03C, typeof(InstructionBox) },

            { Opcode.U_100, typeof(InstructionBox) },
            { Opcode.U_101, typeof(InstructionBox) }, // save and exit? huh?
            { Opcode.U_111, typeof(InstructionBox) },
            { Opcode.INTRA_FILE_JUMP, typeof(IntraFileJump) },
            { Opcode.INTER_FILE_JUMP, typeof(InterFileJump) },
            { Opcode.U_120, typeof(InstructionBox) },
            { Opcode.U_121, typeof(InstructionBox) },
            { Opcode.U_130, typeof(InstructionBox) },
            { Opcode.U_131, typeof(InstructionBox) },
            { Opcode.U_132, typeof(InstructionBox) },
            { Opcode.U_160, typeof(InstructionBox) },
            { Opcode.CHANGE_MUSIC, typeof(InstructionBox) },
            { Opcode.U_171, typeof(InstructionBox) },
            { Opcode.U_173, typeof(InstructionBox) },
            { Opcode.U_174, typeof(InstructionBox) },
            { Opcode.U_182, typeof(InstructionBox) },
            { Opcode.U_190, typeof(InstructionBox) },
            { Opcode.U_191, typeof(InstructionBox) },
            { Opcode.U_192, typeof(InstructionBox) },
            { Opcode.U_1A0, typeof(InstructionBox) }, // view change?
            { Opcode.U_1C0, typeof(InstructionBox) },

            { Opcode.U_205, typeof(InstructionBox) },
            { Opcode.U_210, typeof(InstructionBox) },
            { Opcode.U_211, typeof(InstructionBox) },
            { Opcode.U_214, typeof(InstructionBox) },

            { Opcode.U_302, typeof(InstructionBox) },
            { Opcode.U_303, typeof(InstructionBox) },
            { Opcode.U_30B, typeof(InstructionBox) },
            { Opcode.U_30C, typeof(InstructionBox) },

            { Opcode.U_401, typeof(InstructionBox) },
            { Opcode.WAIT, typeof(InstructionBox) },
            { Opcode.U_403, typeof(InstructionBox) },
            { Opcode.ROTATE_CAMERA, typeof(InstructionBox) },
            { Opcode.U_405, typeof(InstructionBox) },
            { Opcode.U_406, typeof(InstructionBox) },
            { Opcode.U_407, typeof(InstructionBox) },

            { Opcode.BASIC_TEXT_BOX, typeof(BasicTextBox) },
            { Opcode.U_501, typeof(U_501) },
            { Opcode.U_502, typeof(U_502) },
            { Opcode.HIDE_TEXT_BOX, typeof(HideTextBox) },

            { Opcode.SHOW_IMAGE, typeof(ShowImage) }, // a tentative name

            { Opcode.U_601, typeof(InstructionBox) },
            { Opcode.U_602, typeof(InstructionBox) },
            { Opcode.U_603, typeof(InstructionBox) },

            { Opcode.CHOICE_DIALOG, typeof(ChoiceDialog) },

            { Opcode.PLAY_INTERRUPT, typeof(InstructionBox) },

            { Opcode.U_1000, typeof(InstructionBox) },
            { Opcode.LOCATION_CARD, typeof(LocationCard) },

        };


        public static int GetInstructionSize(Opcode opcode)
        {
            int size;
            bool success;

            success = instructionSize.TryGetValue(opcode, out size);

            if (!success)
            {
                string errorMessage = string.Format("No instruction size specified for opcode {0}.", opcode.ToString());
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            return size;
        }

        public static Type GetInstructionParserType(Opcode opcode)
        {

            Type type;
            bool success;

            success = instructionParserMap.TryGetValue(opcode, out type);

            if (!success)
            {
                string errorMessage = string.Format("No instruction parser type specified for opcode {0}.", opcode.ToString());
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            return type;
        }

    }

}
