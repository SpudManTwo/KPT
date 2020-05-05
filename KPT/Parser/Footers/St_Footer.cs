using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.Parser.Elements;

namespace KPT.Parser.Footers
{
    /// <summary>
    /// A varible length footer that appears to be shared by all files in the St family
    /// </summary>
    class St_Footer : IFooter
    {
        DataBox footerContents;

        /// <summary>
        /// Reads the footer of a file
        /// </summary>
        /// <param name="br">The file to be read</param>
        /// <remarks>
        /// Works backwards from the end of the file to calcuate the footer then returns the stream's position back to the start of the stream. Does not preserve the curren position of any streams passed to it.
        /// </remarks>
        public bool Read(BinaryReader br)
        {
            br.BaseStream.Seek(-1, SeekOrigin.End);
            
            int footerSize = 0;            

            if (br.ReadByte() != 0x88)
            {
                br.BaseStream.Seek(0, SeekOrigin.Begin); // is this control flow convoluted?
                footerContents = new DataBox(0);
                footerContents.Read(br);
                return true;
            }

            br.BaseStream.Seek(-1, SeekOrigin.End);

            while (br.ReadByte() == 0x88)
            {
                br.BaseStream.Seek(-2, SeekOrigin.Current);
                footerSize += 1;
            }

            footerContents = new DataBox(footerSize);
            footerContents.Read(br);

            br.BaseStream.Seek(0, SeekOrigin.Begin);

            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            footerContents.Write(bw);
            return true;
        }

    }
}
