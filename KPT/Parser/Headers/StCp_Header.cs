using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPT.Parser.Instructions;
using KPT.Parser.Elements;
using System.IO;

namespace KPT.Parser.Headers
{
    /// <summary>
    /// Used to read the header of files following the StXXX_CpXXXX naming convention.
    /// </summary>
    class StCp_Header : IHeader
    {

        public const int HEADER_SIZE = 0x60;

        StCpNumber fileNumber;
        DataBox box1;

        public StCp_Header()
        {
            fileNumber = new StCpNumber();
        }

        public bool Read(BinaryReader br)
        {
            fileNumber.Read(br);
            box1 = new DataBox(0x5C); // header size minus size of StCp number
            box1.Read(br);
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            fileNumber.Write(bw);
            box1.Write(bw);
            return true;
        }

        public StCpNumber GetFileNumber()
        {
            return fileNumber;
        }

    }
}
