using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KPT.Parser.Instructions;

namespace KPT.Parser
{
    class DynamicTextBoxes
    {

        /// <summary>
        /// Approximate amount of pixels that can fit on screen - a bit below what might realistically fit because BOX_MAX_LENGTH messes things up a bit. 
        /// </summary>
        const int LINE_LENGTH_WIDTH = 280;
        /// <summary>
        /// The number of lines that will fit in a box on the screen
        /// </summary>
        const int BOX_LINES = 3;
        /// <summary>
        /// How many characters will fit in the memory buffer used by the boxes - shared by all lines in the box.
        /// </summary>
        /// <remarks>
        /// This is caused by the buffer being sized so as to be able to fit the maximum possible Japanese text on the screen. Since more Latin characters can be packed into one box, the buffer is too small to hold all of them and so the entire box cannot be utilized. Ideally this restriction will be lifted once I find a way to directly maniuplate the buffer size in a way that can be made permanent.
        /// </remarks>
        const int BOX_MAX_LENGTH = 95;
        /// <summary>
        /// Character to use when attempting to split English text into words - currently based on _ since spaces will be replaced with _ at current due to technical limitations
        /// </summary>
        const char SEGMENT_SEPERATOR = ' ';
        const char CHOSEN_NEWLINE = '\n'; // not actually a choice at the moment - if this gets changed remeber that SplitIntoSegments will remove \r 

        FontHandler fontHandler = new FontHandler();

        public class BoxLines
        {
            public string line1 = string.Empty;
            public string line2 = string.Empty;
            public string line3 = string.Empty;

            public bool IsFull
            {
                get
                {
                    return (line1 != string.Empty && line2 != string.Empty && line3 != string.Empty);
                }
            }

            public bool IsEmpty
            {
                get
                {
                    return (line1 == string.Empty && line2 == string.Empty && line3 == string.Empty);
                }
            }

            public int TotalSize
            {
                get
                {
                    return (line1.Length + line2.Length + line3.Length);
                }
            }

            public void AddLine(string newLine)
            {
                if (line1 == string.Empty)
                {
                    line1 = newLine;
                }
                else if (line2 == string.Empty)
                {
                    line2 = newLine;
                }
                else if (line3 == string.Empty)
                {
                    line3 = newLine;
                }
                else
                {
                    throw new Exception("Tried to add line to full box");
                }
            }

            public override string ToString()
            {
                return line1 + line2 + line3;
            }

        }

        public void CheckTextboxes(KCFile file)
        {

            LinkedList<IInstruction> processedInstructons = new LinkedList<IInstruction>();


            foreach (var instruction in file.instructions)
            {
                if (instruction is IDialogueBox)
                {
                    var temp = instruction as IDialogueBox;
                    string dialogue = temp.GetDialogue();
                    //This forces a translated name check right before insertion. Better to do it here than earlier because it is one of our matching points for where to insert. -Spud
                    temp.SetName(temp.GetName());
                    var boxes = ProcessDialogue(dialogue);
                    foreach (var newBox in BreakUpDialgoueBox(temp, boxes))
                    {
                       processedInstructons.AddLast(newBox);                    
                    }
                }
                else
                {
                    processedInstructons.AddLast(instruction);
                }
            }

            file.instructions = processedInstructons.ToList();

        }

         /// <summary>
         /// Take an IDialogueBox and break it down into multiple instructions as necessary
         /// </summary>
         /// <param name="input"></param>
         /// <returns></returns>
        public IInstruction[] BreakUpDialgoueBox(IDialogueBox input, BoxLines[] boxes)
        {

            /*if (boxes.Length == 1)
            {
                return new IInstruction[1] { input as IInstruction };
            }
            //Leaving this old here for posterity. I removed this out because it was causing issues with text boxes that have 2 lines but no overflow. (They weren't wrapping properly for some reason.)
            */

            List<IInstruction> newBoxes = new List<IInstruction>();

            input.SetDialogue(boxes[0].ToString());
            newBoxes.Add(input as IInstruction);

            for (int i = 1; i < boxes.Length; i++)
            {
                var workingBox = boxes[i];
                BasicTextBox newDialogBox = new BasicTextBox();
                newDialogBox.InitalizeDefault();
                newDialogBox.SetDialogue(workingBox.ToString());
                newDialogBox.SetName(input.GetName());
                newBoxes.Add(newDialogBox);
            }

            return newBoxes.ToArray();
        }

        public BoxLines[] ProcessDialogue(string dialogue)
        {
            var segments = BreakIntoSegments(dialogue);
            var lines = FitOntoLines(segments);
            var boxes = FitOntoBoxes(lines);

            return boxes;   
        }

        public BoxLines[] FitOntoBoxes(string[] lines)
        {
            List<BoxLines> boxes = new List<BoxLines>();

            BoxLines workingBox = new BoxLines();

            foreach (string line in lines)
            {
                string workingLine = line;

                if (workingLine.Length > 1)
                {
                    if (workingLine.Last() == SEGMENT_SEPERATOR)
                    {
                        workingLine = workingLine.Substring(0, workingLine.Length - 1);
                    }

                    if (workingLine.Last() != CHOSEN_NEWLINE)
                    {
                        workingLine = workingLine + CHOSEN_NEWLINE;
                    }
                }

                if (workingBox.IsFull)
                {
                    boxes.Add(workingBox);
                    workingBox = new BoxLines();
                }

                if (workingBox.TotalSize + workingLine.Length > BOX_MAX_LENGTH)
                {
                    boxes.Add(workingBox);
                    workingBox = new BoxLines();
                }

                workingBox.AddLine(workingLine);

            }

            if (!workingBox.IsEmpty)
            {
                boxes.Add(workingBox);
            }

            return boxes.ToArray();
        }

        /// <summary>
        /// Fit the given segments onto lines
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public string[] FitOntoLines(string[] segments)
        {
            List<string> lines = new List<string>();

            string newLine = string.Empty;
            int newLineLength = 0;
            int bufferConsumption = 0;

            foreach (string segment in segments)
            {
                string workingSegment = segment;
                int workingSegmentLength = GetSegmentLength(workingSegment);

                if (workingSegmentLength >= LINE_LENGTH_WIDTH)
                {
                    if (!QuickEnglishTest(workingSegment)) // ignore Japanese segements - we're willing to allow them to be mangled
                    {
                        MessageBox.Show(string.Format("Segment {0} is too large for a line - functionality for splitting segments is yet to be reimplemented", workingSegment));
                        Environment.Exit(0);
                    }
                }

                if (newLineLength + workingSegmentLength > LINE_LENGTH_WIDTH)
                {
                    lines.Add(newLine);
                    bufferConsumption += newLine.Length;
                    newLine = string.Empty;
                    newLineLength = 0;
                }

                if(bufferConsumption + newLine.Length + workingSegment.Length > BOX_MAX_LENGTH)
                {
                    lines.Add(newLine);
                    bufferConsumption = 0;
                    newLine = string.Empty;
                    newLineLength = 0;
                }

                if (segment == CHOSEN_NEWLINE.ToString())
                {
                    newLine += workingSegment;
                    lines.Add(newLine);
                    newLine = string.Empty;
                    newLineLength = 0;
                }
                else
                {
                    newLine += workingSegment;
                    newLineLength += workingSegmentLength;
                }


            }

            if (newLine != string.Empty)
            {
                lines.Add(newLine);
            }

            return lines.ToArray();
        }

        /// <summary>
        /// Break the text down into an array of words
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string[] BreakIntoSegments(string input)
        {

            input = input.Replace("\r", ""); // \r means nothing to us

            List<string> segments = new List<string>();
            string[] segmentArray = input.Split(SEGMENT_SEPERATOR);

            foreach (var segment in segmentArray)
            {

                if (segment.Contains(CHOSEN_NEWLINE))
                {
                    string[] subsegments = segment.Split(CHOSEN_NEWLINE);
                    int numberOfSubSegments = subsegments.Length;
                    for (int i = 0; i < numberOfSubSegments - 1; i++) // trying to deal with the newlines without adding an extra newline to the last element
                    {
                        segments.Add(subsegments[i] + SEGMENT_SEPERATOR);
                        segments.Add(CHOSEN_NEWLINE.ToString());
                    }
                    segments.Add(subsegments[numberOfSubSegments - 1] + SEGMENT_SEPERATOR);
                }
                else
                {
                    segments.Add(segment + SEGMENT_SEPERATOR);
                }
            }

            return segments.ToArray();
        }

        public int GetSegmentLength(string segment)
        {
            return fontHandler.CalcuateSegmentWidth(segment);
        }

        /// <summary>
        /// Make a quick guess on whether or not something is English text or Japanese text by measuring the number of bytes in the a-zA-z ASCII range
        /// </summary>
        /// <param name="input"></param>
        /// <returns>True on probably English, false on probably Japanese</returns>
        private bool QuickEnglishTest(string input)
        {
            int count = 0;
            int length = input.Length;

            foreach (char letter in input)
            {
                if (letter > 0x40 && letter < 0x7B)
                {
                    count++;
                }
            }

            return (count < length / 2);

        }

    }

}
