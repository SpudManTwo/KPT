using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.Parser.Spreadsheet_Interface;

namespace KPT.Parser
{
    /// <summary>
    /// Represents an instruction in a file - usually an opcode and arguments
    /// </summary>
    interface IInstruction : IElement
    {
        /// <summary>
        /// Read an instruction and its arguments from the given stream
        /// </summary>
        /// <param name="br">The stream from which to read</param>
        /// <returns>True on successful read, false on failed read</returns>
        new bool Read(BinaryReader br);
        /// <summary>
        /// Write an instruction and its arguments to the given stream
        /// </summary>
        /// <param name="bw">The stream to write to</param>
        /// <returns>True on successful write, false on failed write</returns>
        new bool Write(BinaryWriter bw);
    }

    /// <summary>
    /// Represents the header of a file
    /// </summary>
    interface IHeader : IElement
    {
        new bool Read(BinaryReader br);
        new bool Write(BinaryWriter bw);
    }

    /// <summary>
    /// Represents the footer of a file
    /// </summary>
    interface IFooter : IElement
    {
        new bool Read(BinaryReader br);
        new bool Write(BinaryWriter bw);
    }

    /// <summary>
    /// The base interface for all elements - represents a block of data in a file
    /// </summary>
    interface IElement
    {
        bool Read(BinaryReader br);
        bool Write(BinaryWriter bw);
    }

    /// <summary>
    /// Given to elements that represent dialogue boxes with a name string to help populate the name list
    /// </summary>
    interface IHasName
    {
        string GetName();
        void SetName(string newName);
    }

    /// <summary>
    /// Given to elements that have strings not explictly tied to existing StringCollection (such as the name collection) 
    /// </summary>
    interface IHasStrings
    {
        /// <summary>
        /// Have the element add its strings to the string collection
        /// </summary>
        /// <param name="collection"></param>
        void AddStrings(StringCollection collection);
        /// <summary>
        /// Have the element pull its strings form the collection
        /// </summary>
        /// <param name="collection"></param>
        void GetStrings(StringCollection collection);
        /// <summary>
        /// Have the element create CSV records for itself
        /// </summary>
        /// <returns></returns>
        List<CSVRecord> GetCSVRecords();
    }

    /// <summary>
    /// Interface providing information needed for dialogue boxes
    /// </summary>
    interface IDialogueBox : IHasName
    {
        bool isTranslated
        {
            get;
            set;
        }

        string GetDialogue();
        void SetDialogue(string newDialogue);

    }

}
