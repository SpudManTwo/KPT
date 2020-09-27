using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptFileMapper
{
    class Program
    {
        //Version Constants
        const string VERSION = "2020911K";

        //Int Constants
        const short DESCRIPTOR_LBA = 16; // 16 = 0x010 = LBA of the first volume descriptor
        const short TOTAL_SECTORS = 80; // 80 = 0x050 = total number of sectors in the UMD
        const short ROOT_FOLDER_LBA = 158; // 158 = 0x09E LBA of the root folder
        const short ROOT_SIZE = 166; // 166 = 0x0A6 size of the root folder
        const short TABLE_PATH_LEN = 132; // 132 = 0x084 size of the first table path
        const short TABLE_PATH_LBA = 140; // 132 = 0x08C LBA of the first table path

        //Long Constants
        const ulong DESCRIPTOR_SIG_1 = 335558014681857; // 335558014681857 = 0x0001313030444301ULL volume descriptor signatures
        const ulong DESCRIPTOR_SIG_2 = 335558014682111; // 335558014681857 = 0x00013130304443FFULL

        //M0 Constants
        const uint sectorSize = 2048;// 2048 = 0x800 = LEN_SECTOR_M0 = sector size
        const uint sectorData = 2048; // 2048 = 0x800 = POS_DATA_M0 = sector data length
        const uint dataOffset = 0; // 2048 = 0x800 = LEN_DATA_M0 = sector data start

        //File Input/Output Constants
        const string TMPNAME = "umd-replacek.$"; // temporal name
        const int BLOCKSIZE = 16384; // sectors to read/write at once

        //Non-Constant Ints
        ushort mode;        // image mode

        //Exit Code Definitions as per https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes?redirectedfrom=MSDN
        const int SUCCESS_EXIT_CODE = 0;
        const int BAD_APPLICATION_ARGUMENTS_EXIT_CODE = 160;
        const int FILE_NOT_FOUND_EXIT_CODE = 2;
        const int FILE_NAME_TOO_LONG = 111;
        const int CANNOT_OPEN_FILE_EXIT_CODE = 110;
        
        static void Main(string[] args)
        {
            //Title
            System.Console.WriteLine("\nUMD-REPLACE version %s - Copyright (C) 2012-2015 CUE"
                                    +"\nTiny tool to replace data files in a PSP UMD ISO\n"
                                    + VERSION);

            //Arg Check
            if(args.Length != 3)
            {
                //Usage Explanation
                System.Console.WriteLine("Usage: UMD-REPLACE imagename filename newfile\n"
                                        + "\n- 'imagename' is the name of the UMD image"
                                        + "\n- 'filename' is the file in the UMD image with the data to be replaced"
                                        + "\n- 'newfile' is the file with the new data"
                                        + "\n"
                                        + "\n* 'imagename' must be a valid UMD ISO image"
                                        + "\n* 'filename' can use either the slash or backslash"
                                        + "\n* 'newfile' can be different size as 'filename'");
                //Bad Argument Exit
                Environment.Exit(BAD_APPLICATION_ARGUMENTS_EXIT_CODE);
            }

            //Run the Replace
            Replace(args[0], args[1], args[2]);

            System.Console.WriteLine("\nDone");

            //Successful Exit
            Environment.Exit(SUCCESS_EXIT_CODE);
        }

        static void Replace(string isoName, string oldName, string newName)
        {
            FileStream fs = null;
            try
            {
                fs = File.Open(isoName, FileMode.Open);
            }
            catch (ArgumentException)
            {
                System.Console.WriteLine("No such iso file '" + isoName + "'\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }
            catch (PathTooLongException)
            {
                System.Console.WriteLine("Iso File Path '" + isoName + "' exceeds system-defined length.\n");
                Environment.Exit(FILE_NAME_TOO_LONG);
            }
            catch (DirectoryNotFoundException)
            {
                System.Console.WriteLine("Specified iso path'" + isoName + "' is invalid.\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }
            catch (UnauthorizedAccessException)
            {
                System.Console.WriteLine("Current User lacks sufficient permissions for iso file '" + isoName + "'\n");
                Environment.Exit(CANNOT_OPEN_FILE_EXIT_CODE);
            }
            catch (FileNotFoundException)
            {
                System.Console.WriteLine("No such iso file '" + isoName + "'\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }
            catch (NotSupportedException)
            {
                System.Console.WriteLine("Iso File '" + isoName + "' is not in a recognizable format.\n");
                Environment.Exit(CANNOT_OPEN_FILE_EXIT_CODE);
            }
            catch (IOException)
            {
                System.Console.WriteLine("A error occured while opening the iso file '" + isoName + "'\n");
                Environment.Exit(CANNOT_OPEN_FILE_EXIT_CODE);
            }

            List<byte> originalIsoFile = new List<byte>();
            //Open File for reading into Buffer
            using (BinaryReader reader = new BinaryReader(fs))
            {
                originalIsoFile.AddRange(reader.ReadBytes((int)reader.BaseStream.Length));
            }

            fs.Close();

            try
            {
                fs = File.Open(isoName, FileMode.Open);
            }
            catch (ArgumentException)
            {
                System.Console.WriteLine("No such input file for '" + isoName + "'\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }
            catch (PathTooLongException)
            {
                System.Console.WriteLine("Input File Path '" + isoName + "' exceeds system-defined length.\n");
                Environment.Exit(FILE_NAME_TOO_LONG);
            }
            catch (DirectoryNotFoundException)
            {
                System.Console.WriteLine("Specified input file path'" + isoName + "' is invalid.\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }
            catch (UnauthorizedAccessException)
            {
                System.Console.WriteLine("Current User lacks sufficient permissions for input file '" + isoName + "'\n");
                Environment.Exit(CANNOT_OPEN_FILE_EXIT_CODE);
            }
            catch (FileNotFoundException)
            {
                System.Console.WriteLine("No such input file '" + isoName + "'\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }
            catch (NotSupportedException)
            {
                System.Console.WriteLine("Input File '" + isoName + "' is not in a recognizable format.\n");
                Environment.Exit(CANNOT_OPEN_FILE_EXIT_CODE);
            }
            catch (IOException)
            {
                System.Console.WriteLine("A error occured while opening the input file '" + isoName + "'\n");
                Environment.Exit(CANNOT_OPEN_FILE_EXIT_CODE);
            }

            List<byte> newFileBytes = new List<byte>();
            //Open File for reading into Buffer
            using (BinaryReader reader = new BinaryReader(fs))
            {
                newFileBytes.AddRange(reader.ReadBytes((int)reader.BaseStream.Length));                
            }

            fs.Close();
            

            // get data from the primary volume descriptor
            uint imageSectors = BitConverter.ToUInt32(originalIsoFile.GetRange((int)(DESCRIPTOR_LBA + dataOffset + TOTAL_SECTORS), 4).ToArray());
            uint rootLba = BitConverter.ToUInt32(originalIsoFile.GetRange((int)(DESCRIPTOR_LBA + dataOffset + ROOT_FOLDER_LBA), 4).ToArray());
            uint rootLength = BitConverter.ToUInt32(originalIsoFile.GetRange((int)(DESCRIPTOR_LBA + dataOffset + ROOT_SIZE), 4).ToArray());

            foreach(byte b in originalIsoFile.GetRange((int)(DESCRIPTOR_LBA), (int)sectorSize))
            {
                System.Console.WriteLine("byte: " + b);
            }

            // get new data from the new file
            uint newFilesize = (uint)newFileBytes.Count;
            uint newFileSectors = (uint)((newFilesize / sectorData - 1) / sectorData);

            // 'oldName' must start with a path separator
            if ((oldName[0] != '/') && (oldName[0] != '\\'))
            {
                oldName = '/'+oldName;
            }

            // change all backslashes by slashes in 'oldname'
            oldName.Replace("\\", "/");

            // search 'oldname' in the image
            ulong foundPosition = Search(originalIsoFile, oldName, "", rootLba, rootLength);

            if(foundPosition == 0)
            {
                System.Console.WriteLine("Could not find file '" + oldName + "' inside of iso file '"+isoName+"'\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }

            uint foundLBA = (uint)(foundPosition / sectorSize);
            uint foundOffset = (uint)(foundPosition % sectorSize);

            // get data from the old file
            uint oldFilesize = BitConverter.ToUInt32(originalIsoFile.GetRange((int)(foundOffset + 10), 4).ToArray()); //0x0A = 10
            uint oldFileSectors = (oldFilesize + sectorData - 1) / sectorData;
            uint fileLBA = BitConverter.ToUInt32(originalIsoFile.GetRange((int)(foundOffset + 2), 4).ToArray()); //0x02 = 2

            //size difference in sectors
            int diff = BitConverter.ToInt32(BitConverter.GetBytes(newFileSectors - oldFileSectors));

            //image name
            string name = diff == 0 ? isoName : TMPNAME;

            //As a change from the original C code, we won't be creating a duplicate file just for reading as we update since we're storing the whole file in memory.
            //That said, I have left the equivalent C# code commented out below if you want that functionality brought back for some reason.
            uint lba = fileLBA;

            /*
            if (diff != 0)
            {
                //create the new image
                System.Console.WriteLine("- creating temporal image");

                FileStream outputStream = File.Create(name);

                uint lba = 0;

                //update the previous sectors
                System.Console.WriteLine("- updating previous data sectors");

                uint maxim = fileLBA;
                uint count = 0;

                for(uint i=0;i < fileLBA; i += count, lba += count)
                {
                    count = maxim >= BLOCKSIZE ? BLOCKSIZE : maxim; maxim -= count;

                    using (BinaryWriter writer = new BinaryWriter(outputStream))
                    {
                        writer.Write(originalIsoFile.GetRange((int)i, (int)count).ToArray());
                    }
                }
            }
            else 
            {
                lba = fileLBA;
            }
            */

            // update the new file
            System.Console.WriteLine("- updating file data");

            if (newFileSectors != 0)
            {
                /*
                // read and update all data sectors except the latest one (maybe incomplete)
                uint maxim = --newFileSectors;
                uint count = maxim >= BLOCKSIZE ? BLOCKSIZE : maxim;
                for (uint i = 0; i < newFileSectors; i+= count)
                {
                    count = maxim >= BLOCKSIZE ? BLOCKSIZE : maxim;
                    maxim -= count;

                    for (uint j = 0; j < count; j++)
                    {
                        for (uint k = 0; k < sectorData; k++)
                        {

                        }
                    }

                    lba += count;
                }*/

                //Remove the old bytes.
                originalIsoFile.RemoveRange((int)fileLBA, (int)oldFileSectors);
                //Inject the new ones in to the place where the old ones used to sit.
                originalIsoFile.InsertRange((int)fileLBA, newFileBytes);
            }

            uint littleEndian = 0;
            uint bigEndian = 0;

            if(newFilesize != oldFilesize)
            {
                // update the file size
                System.Console.WriteLine("- updating file size");

                littleEndian = newFilesize;
                bigEndian = ChangeEndian(littleEndian);

                //Replace the old little endian bytes
                originalIsoFile.RemoveRange((int)(foundLBA + foundOffset + 10), 4); //0x0A = 10
                originalIsoFile.InsertRange((int)(foundLBA + foundOffset + 10), BitConverter.GetBytes(littleEndian));
                //Replace the old big endian bytes
                originalIsoFile.RemoveRange((int)(foundLBA + foundOffset + 14), 4); //0x0E = 14 
                originalIsoFile.InsertRange((int)(foundLBA + foundOffset + 14), BitConverter.GetBytes(bigEndian));
            }
            
            if(diff != 0)
            {
                // update the primary volume descriptor
                System.Console.WriteLine("- updating primary volume descriptor");

                littleEndian = (uint)(imageSectors + diff);
                bigEndian = ChangeEndian(littleEndian);

                //Replace the old little endian bytes
                originalIsoFile.RemoveRange((int)(DESCRIPTOR_LBA + dataOffset + TOTAL_SECTORS), 4);
                originalIsoFile.InsertRange((int)(DESCRIPTOR_LBA + dataOffset + TOTAL_SECTORS), BitConverter.GetBytes(littleEndian));
                //Replace the old big endian bytes
                originalIsoFile.RemoveRange((int)(DESCRIPTOR_LBA + dataOffset + TOTAL_SECTORS + 4), 4);
                originalIsoFile.InsertRange((int)(DESCRIPTOR_LBA + dataOffset + TOTAL_SECTORS + 4), BitConverter.GetBytes(bigEndian));

                // update the path tables
                System.Console.WriteLine("- updating path tables");

                //Unsure about this next block
                for(int i = 0;i < 4; i++)
                {
                    uint tblLen = BitConverter.ToUInt32(originalIsoFile.GetRange((int)(DESCRIPTOR_LBA + dataOffset + TABLE_PATH_LEN), 4).ToArray());
                    uint tblLBA = BitConverter.ToUInt32(originalIsoFile.GetRange((int)(DESCRIPTOR_LBA + dataOffset + TABLE_PATH_LBA + 4 * i), 4).ToArray());
                    if(tblLBA != 0)
                    {
                        if(i == 0x2) //Bit wise & for byte comparison in C.
                        {
                            tblLBA = ChangeEndian(tblLBA);
                        }
                        PathTable(originalIsoFile, tblLBA, tblLen, fileLBA, diff, i == 0x2);
                    }
                }
            }

            // update the file/folder LBAs
            System.Console.WriteLine("- updating entire TOCs");

            TOC(originalIsoFile, rootLba, rootLength, foundPosition, fileLBA, diff);

            //Rename the old file to a temporal file
            System.IO.File.Move(isoName, isoName+TMPNAME);

            //Write out the new file.
            FileStream outputStream = null;
            try
            {
                outputStream = File.Open(isoName, FileMode.Create);
            }
            catch (ArgumentException)
            {
                System.Console.WriteLine("No such iso file '" + isoName + "'\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }
            catch (PathTooLongException)
            {
                System.Console.WriteLine("Iso File Path '" + isoName + "' exceeds system-defined length.\n");
                Environment.Exit(FILE_NAME_TOO_LONG);
            }
            catch (DirectoryNotFoundException)
            {
                System.Console.WriteLine("Specified iso path'" + isoName + "' is invalid.\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }
            catch (UnauthorizedAccessException)
            {
                System.Console.WriteLine("Current User lacks sufficient permissions for iso file '" + isoName + "'\n");
                Environment.Exit(CANNOT_OPEN_FILE_EXIT_CODE);
            }
            catch (FileNotFoundException)
            {
                System.Console.WriteLine("No such iso file '" + isoName + "'\n");
                Environment.Exit(FILE_NOT_FOUND_EXIT_CODE);
            }
            catch (NotSupportedException)
            {
                System.Console.WriteLine("Iso File '" + isoName + "' is not in a recognizable format.\n");
                Environment.Exit(CANNOT_OPEN_FILE_EXIT_CODE);
            }
            catch (IOException)
            {
                System.Console.WriteLine("A error occured while opening the iso file '" + isoName + "'\n");
                Environment.Exit(CANNOT_OPEN_FILE_EXIT_CODE);
            }

            //Writing Temporal File
            using (BinaryWriter binaryWriter = new BinaryWriter(outputStream))
            {
                binaryWriter.Write(originalIsoFile.ToArray());
            }

            outputStream.Close();

            System.Console.Write("- the new image has ");
            if(diff > 0)
            {
                System.Console.WriteLine(diff + " more sectors than the original image.");
            }
            else if(diff < 0)
            {
                System.Console.WriteLine(diff + " less sectors than the original image.");
            }
            else
            {
                System.Console.WriteLine("the same amount of sectors as the original image.");
            }

            if (diff != 0)
            {
                System.Console.WriteLine("- maybe you need to hand update the cuesheet file (if exist and needed)");
            }
        }

        static ulong Search(List<byte> originalIso, string fileName, string path, uint lba, uint len)
        {
            ulong totalSectors = (ulong)((len + sectorSize - 1) / sectorSize);
            ulong found = 0;
            for(uint i = 0; i < totalSectors; i++)
            {
                uint nBytes = 0;
                for (long position = 0; position < sectorData && (dataOffset + position + 4) < originalIso.Count; position += nBytes)
                {
                    //field size
                    nBytes = BitConverter.ToChar(originalIso.GetRange((int)(lba + i + dataOffset + position), 1).ToArray());

                    //name size
                    char nChars = BitConverter.ToChar(originalIso.GetRange((int)(lba + i + dataOffset + position + 32), 1).ToArray()); //0x020 = 32
                    string name = "";
                    for (int j = 0; j < nChars; j++)
                    {
                        name += BitConverter.ToChar(originalIso.GetRange((int)(lba + i + dataOffset + position + 33 + j), 1).ToArray()); //0x021 = 33
                    }
                    System.Console.WriteLine("Position: " + position + " Name: " + name);

                    // discard the ";1" final
                    if (nChars > 2)
                    {
                        name = name.Substring(0, name.Length - 2);
                    }

                    // check the name except for '.' and '..' entries
                    if ((nChars != 1) || ((name[0] != '\0') && (name[0] != '1')))
                    {
                        //new path name
                        string newPath = string.Format("{0}/{1}", path, name); // sprintf is the string format for C. While the syntax looks different, the functionality is supposed to be the same.
                        if (originalIso[(int)(dataOffset + position + 25)] == 0x02) { // 0x019 = 25, Bitwise & in C compares two bytes for equality.
                            
                            uint newLBA = BitConverter.ToChar(originalIso.GetRange((int)(lba + i + dataOffset + position + 2), 4).ToArray()); // 0x002 = 2
                            uint newLen = BitConverter.ToChar(originalIso.GetRange((int)(lba + i + dataOffset + position + 10), 4).ToArray()); // 0x00A = 10

                            found = Search(originalIso, fileName, newPath, newLBA, newLen);
                            if(found != 0)
                            {
                                return found;
                            }
                            else if(fileName.Equals(newPath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return (lba + i) * (ulong)(sectorSize + dataOffset + position);
                            }
                        }
                    }
                }
            }

            //if not found return 0
            return 0;
        }

        static void PathTable(List<byte> originalIso, uint lba, uint len, uint lbaOld, int diff, bool sw)
        {
            uint nBytes = 0;

            for (uint pos = 0; pos < len; pos = 8 + nBytes + (nBytes & 0x1)) //0x08 = 8 Oh and this one is actually used as a bit-wise AND
            {
                //field size
                nBytes = BitConverter.ToUInt32(originalIso.GetRange((int)(lba + dataOffset + pos), 4).ToArray());
                if (nBytes == 0)
                {
                    //no more entries in table
                    break;
                }

                //position
                uint newLBA = BitConverter.ToUInt32(originalIso.GetRange((int)(lba + dataOffset + pos + 2), 4).ToArray()); //0x002 = 2
                if(sw)
                {
                    newLBA = ChangeEndian(newLBA);
                }

                //update needed
                if(newLBA > lbaOld)
                {
                    newLBA += (uint)diff;
                    if(sw)
                    {
                        newLBA = ChangeEndian(newLBA);
                    }

                    //update sectors if needed, 0x002 = 2
                    originalIso.RemoveRange((int)(lba + dataOffset + pos + 2), 4);
                    originalIso.InsertRange((int)(lba + dataOffset + pos + 2), BitConverter.GetBytes(newLBA));
                }                
            }
        }

        static void TOC(List<byte> originalIso, uint lba, uint len, ulong found, uint lbaOld, int diff)
        {
            // total sectors
            long totalSectors = originalIso.Count / sectorSize;


            for(uint i = 0; i < totalSectors; i++)
            {
                uint nBytes = 0;
                for(uint pos = 0; pos < len; pos += nBytes)
                {
                    //field size
                    nBytes = BitConverter.ToUInt32(originalIso.GetRange((int)(lba + i + dataOffset + pos), 4).ToArray());
                    if(nBytes == 0)
                    {
                        //no more entries in this sector;
                        break;
                    }

                    //name size
                    char nChars = BitConverter.ToChar(originalIso.GetRange((int)(lba + i + dataOffset + pos + 32), 1).ToArray()); //0x020 = 32
                    string name = "";
                    for (int j = 0; j < nChars; j++)
                    {
                        name += BitConverter.ToChar(originalIso.GetRange((int)(lba + i + dataOffset + pos + 33 + j), 1).ToArray()); //0x021 = 33
                    }

                    // position
                    uint newLBA = BitConverter.ToUInt32(originalIso.GetRange((int)(lba + dataOffset + pos + 2), 4).ToArray()); // 0x002 = 2

                    // needed to change a 0-bytes file with more 0-bytes files (same LBA)
                    ulong newfound = (ulong)(lba + i) * sectorSize + dataOffset + pos;

                    if ((newLBA > lbaOld) || ((newLBA == lbaOld) && (newfound > found)))
                    {
                        //update sector if needed
                        newLBA += (uint)diff;

                        originalIso.RemoveRange((int)(lba + i + dataOffset + pos + 2), 4); // 0x002 = 2
                        originalIso.InsertRange((int)(lba + i + dataOffset + pos + 2), BitConverter.GetBytes(newLBA));

                        originalIso.RemoveRange((int)(lba + i + dataOffset + pos + 6), 4); // 0x006 = 6
                        originalIso.InsertRange((int)(lba + i + dataOffset + pos + 6), BitConverter.GetBytes(ChangeEndian((uint)name.Length))); //typically this would be j but that j = length of string and C# has things for that.
                    }

                    // check the name except for '.' and '..' entries
                    if ((nChars != 1) || ((name[0] != '\0') && (name[0] != '1')))
                    {
                        // recursive update in folders
                        if (BitConverter.ToChar(originalIso.GetRange((int)(lba + i + dataOffset + pos + 25), 1).ToArray()) == 0x02) //0x019 = Using bit wise & as a comparison again here.
                        {
                            uint newLen = BitConverter.ToUInt32(originalIso.GetRange((int)(lba + dataOffset + pos + 10), 4).ToArray()); // 0x00A = 2

                            TOC(originalIso, newLBA, newLen, found, lbaOld, diff);
                        }
                    }
                }
            }
        }

        static uint ChangeEndian(uint value)
        {
            return BitConverter.ToUInt32((byte[])BitConverter.GetBytes(value).Reverse());
        }
    }
}
