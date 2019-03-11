﻿using System;
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
        /// How many characters will fit on the screen (an approximation since the font does not have a uniform character size and cannot be readily examined at current)
        /// </summary>
        /// <remarks>
        /// The line length should be 1 less than the actual intended length on screen so as to make space for new lines being added in
        /// </remarks>
        const int LINE_LENGTH = 44;
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
        const int BOX_MAX_LENGTH = 90;
        /// <summary>
        /// Character to use when attempting to split English text into words - currently based on _ since spaces will be replaced with _ at current due to technical limitations
        /// </summary>
        const char SEGMENT_SEPERATOR = '_';
        const char CHOSEN_NEWLINE = '\n';

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
                    var boxes = ProcessDialogue(dialogue);
                    if (boxes.Length == 1)
                    {
                        processedInstructons.AddLast(instruction);
                    }
                    else
                    {
                       foreach (var newBox in BreakUpDialgoueBox(temp, boxes))
                        {
                            processedInstructons.AddLast(newBox);
                        }
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

            if (boxes.Length == 1)
            {
                return new IInstruction[1] { input as IInstruction };
            }

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

            foreach (string segment in segments)
            {
                string workingSegment = segment;

                if (newLine.Length + workingSegment.Length > LINE_LENGTH)
                {
                    lines.Add(newLine);
                    newLine = string.Empty;
                }
                newLine += workingSegment;
            }

            if (newLine != string.Empty)
            {
                lines.Add(newLine);
            }

            return lines.ToArray();
        }

        /// <summary>
        /// Break the text down into an array of words (while also breaking down extremely long words into mangable chunks)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string[] BreakIntoSegments(string input)
        {
            List<string> segments = new List<string>();
            string[] segmentArray = input.Split(SEGMENT_SEPERATOR);

            // making sure there are no segments large enough they take up more than an entire line
            foreach (string segment in segmentArray)
            {
                string workingSegment = segment + SEGMENT_SEPERATOR;

                while (workingSegment.Length > LINE_LENGTH)
                {
                    string newLine = workingSegment.Substring(0, LINE_LENGTH);
                    string remainingSegment = workingSegment.Substring(LINE_LENGTH, workingSegment.Length-LINE_LENGTH);
                    segments.Add(newLine);
                    workingSegment = remainingSegment;
                }
                segments.Add(workingSegment);
            }

            return segments.ToArray();
        }

    }
}
