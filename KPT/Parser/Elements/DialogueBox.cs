using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.Parser.Instructions;
using KPT.Parser.Spreadsheet_Interface;

namespace KPT.Parser.Elements
{
    class DialogueBox : IElement, IHasName, IHasStrings, IDialogueBox
    {
        string name;
        string dialogue;
        bool translated = false;
        static List<string> failedTranslatedNames = new List<string>();

        public bool Read(BinaryReader br)
        {
            name = FileIOHelper.ReadName(br);
            dialogue = FileIOHelper.ReadDialogueString(br);
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            FileIOHelper.WriteFixedLengthString(bw, name, Constants.NAME_LENGTH);
            FileIOHelper.WriteDialogueString(bw, dialogue);
            return true;
        }

        public string GetName()
        {
            return name;
        }

        public void SetName(string newName)
        {
            string temp = Constants.TRANSLATED_NAME_TAGS.ContainsKey(newName) ? Constants.TRANSLATED_NAME_TAGS[newName] : newName;
            if(temp.Equals(newName) && !failedTranslatedNames.Contains(newName) && !Constants.TRANSLATED_NAME_TAGS.ContainsValue(newName))
            {
                //TODO Remove before final release.
                //We failed to translate the nametag. This is only for development and should be removed for the final release
                failedTranslatedNames.Add(newName);
            }
            name = temp;
        }

        public void AddStrings(StringCollection collection)
        {
            string newID = collection.GenerateNewID();
            collection.AddString(newID, dialogue);
            dialogue = newID;
        }

        public void GetStrings(StringCollection collection)
        {
            dialogue = collection.GetString(dialogue);
            isTranslated = true; // We're assuming that the primary reason for the strings changing is loading translated strings - it's possible this may not be the case, but it is very unlikely
        }

        public List<CSVRecord> GetCSVRecords()
        {
            return new List<CSVRecord> { new CSVRecord(name, dialogue) };
        }

        public bool isTranslated
        {
            get { return translated; }
            set { translated = value; }
        }

        public string GetDialogue()
        {
            return dialogue;
        }

        public void SetDialogue(string newDialogue)
        {
            dialogue = newDialogue;
        }

    }
}
