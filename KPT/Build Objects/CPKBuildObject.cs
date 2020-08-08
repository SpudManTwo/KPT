using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPT;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using SharpYaml;
using LibCPK;

namespace KPT.BuildObjects
{

    
    /// <summary>
    /// The necessary data for keeping track of a file within a CPK
    /// </summary>
    class CPKEmbeddedFileMeta
    {
        public string fileName;
        public string filePath;
        public bool shouldBeCompressed = false;
        public uint ID;
        public string checksumValue;
        public Checksum checksumType;
        
    }

    /// <summary>
    /// Keeps track of the data necessary to build a CPK
    /// </summary>
    class CPKBuildObject
    {
        [SharpYaml.Serialization.YamlMember] // this makes it write private members
        string originalFileLocation;
        [SharpYaml.Serialization.YamlMember]
        string targetFileLocation;
        [SharpYaml.Serialization.YamlMember]
        Dictionary<uint, CPKEmbeddedFileMeta> files;

        public static List<string> changedCPKs = new List<string>();

        public CPKBuildObject()
        {
            files = new Dictionary<uint, CPKEmbeddedFileMeta>();
        }

        public void SetOriginalFileLocation(string filePath)
        {
            originalFileLocation = filePath;
        }

        public void SetTargetFileLocation(string filePath)
        {
            targetFileLocation = filePath;
        }

        public void AddFile(uint id, CPKEmbeddedFileMeta cpk)
        {
            files[id] = cpk;
        }

        public void SerializeToDisk(string targetFile)
        {
            var yamlSerialzer = new SharpYaml.Serialization.Serializer();
            var serializedData = yamlSerialzer.Serialize( this);

            string targetPath = Path.Combine(targetFile, GenerateCPKID() + ".yaml");
            DirectoryGuard.CheckDirectory(targetPath);

            FileStream fs = new FileStream(targetPath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(serializedData);

            sw.Close();
            fs.Close();
        }

        private string GenerateCPKID()
        {
            return originalFileLocation.Replace(Path.DirectorySeparatorChar, '-').ToLowerInvariant();
        }

        public bool DeserializeFromDisk(string targetFile)
        {
            if (!File.Exists(targetFile))
            {
                string errorMessage = string.Format("File {0} did not exist.", targetFile);
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var yamlSerializer = new SharpYaml.Serialization.Serializer();
            FileStream fs;

            

            try
            {
                fs = new FileStream(targetFile, FileMode.Open);
            }
            catch (Exception e)
            {
                string errorMessage = string.Format("There was an error when opening file {0}.\r\n\r\n{1}.", targetFile, e.Message);
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            StreamReader sr = new StreamReader(fs);
            string data = sr.ReadToEnd();
            files = new Dictionary<uint, CPKEmbeddedFileMeta>();

            try
            {
                yamlSerializer.DeserializeInto<CPKBuildObject>(data, this);
            }
            catch (Exception e)
            {
                string errorMessage = string.Format("There was an error when parsing file {0}.\r\n\r\n{1}.", targetFile, e.Message);
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // might want to specifically check all fields for being null here even if it is a very unlikely edge case

            sr.Close();
            fs.Close();

            return true;
        }

        public bool BuildCPK(string buildFile)
        {

            if (!DeserializeFromDisk(buildFile))
            {
                return false;
            }

            List<CPKEmbeddedFileMeta> changedFiles = new List<CPKEmbeddedFileMeta>();

            foreach (var file in files.Values)
            {
                string filePath = Path.Combine(ProjectFolder.GetRootDir(), file.filePath);

                if (!File.Exists(filePath))
                {
                    string errorMessage = string.Format("File {0} did not exist.", filePath);
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                FileStream fs = new FileStream(filePath, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);

                byte[] fileAsBytes = br.ReadBytes((int)br.BaseStream.Length);

                br.Close();
                fs.Close();

                string checksum = Checksums.GetMD5(fileAsBytes);

               
                if (checksum != file.checksumValue || filePath.EndsWith("St000_ChptDat.bin")) // file has been changed
                {
                    changedFiles.Add(file);
                }

            }

            string originalCPKPath = Path.Combine(ProjectFolder.GetRootDir(), originalFileLocation);
            string targetCPKPath = Path.Combine(ProjectFolder.GetRootDir(), targetFileLocation);

            if (DebugSettings.REFRESH_REPACKED_FILES_ON_BUILD)
            {
                if (File.Exists(targetCPKPath)) // refresh files in repacked files folder just in case since 1) we will be using the repacked folder as our base to merge new changes into 2) a rebuild implies a refresh. perhaps some distinction between "build" and "clean build" would be a good idea later, which case it will most likely derive from this.
                {
                    File.Delete(targetCPKPath);
                }
                else
                {
                    DirectoryGuard.CheckDirectory(targetCPKPath);
                }
                File.Copy(originalCPKPath, targetCPKPath);
            }

            if (changedFiles.Count > 0)
            {
                var batchReplaceArg = new Dictionary<string, string>();

                foreach (var file in changedFiles)
                {
                    string fileName = file.fileName;

                    if (!fileName.Contains("/")) // done to match the format used by the batch replacer
                    {
                        fileName = "/" + fileName;
                    }

                    batchReplaceArg[fileName] = Path.Combine(ProjectFolder.GetRootDir(), file.filePath);
                }

                changedCPKs.Add(targetFileLocation);
                ReplaceCPKFiles(originalCPKPath, targetCPKPath, batchReplaceArg);
            }


            return true;
        }

        // Taken from CriPakTools - see adknowledgements
        private void ReplaceCPKFiles(string sourceCPK, string targetCPK, Dictionary<string, string> batchFileList)
        {

            CPK cpk = new CPK(new Tools());
            cpk.ReadCPK(sourceCPK, ActiveEncodings.currentEncoding);

            BinaryReader oldFile = new BinaryReader(File.OpenRead(sourceCPK));
            bool bUseCompress = false; // this could cause problems later if access to compressed files is needed

            FileInfo fi = new FileInfo(sourceCPK);

            string outputName = targetCPK;

            BinaryWriter newCPK = new BinaryWriter(File.OpenWrite(outputName));

            List<FileEntry> entries = cpk.FileTable.OrderBy(x => x.FileOffset).ToList();

            Tools tool = new Tools();
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].FileType != "CONTENT")
                {

                    if (entries[i].FileType == "FILE")
                    {
                        // I'm too lazy to figure out how to update the ContextOffset position so this works :)
                        if ((ulong)newCPK.BaseStream.Position < cpk.ContentOffset)
                        {
                            ulong padLength = cpk.ContentOffset - (ulong)newCPK.BaseStream.Position;
                            for (ulong z = 0; z < padLength; z++)
                            {
                                newCPK.Write((byte)0);
                            }
                        }
                    }

                    string currentName = ((entries[i].DirName != null) ? entries[i].DirName + "/" : "") + entries[i].FileName;

                    if (!currentName.Contains("/"))
                    {
                        currentName = "/" + currentName;
                    }

                    if (!batchFileList.Keys.Contains(currentName.ToString()))
                    //如果不在表中，复制原始数据
                    {
                        oldFile.BaseStream.Seek((long)entries[i].FileOffset, SeekOrigin.Begin);

                        entries[i].FileOffset = (ulong)newCPK.BaseStream.Position;

                        if (entries[i].FileName.ToString() == "ETOC_HDR")
                        {

                            cpk.EtocOffset = entries[i].FileOffset;
                            Console.WriteLine("Fix ETOC_OFFSET to {0:x8}", cpk.EtocOffset);

                        }

                        cpk.UpdateFileEntry(entries[i]);

                        byte[] chunk = oldFile.ReadBytes(Int32.Parse(entries[i].FileSize.ToString()));
                        newCPK.Write(chunk);

                        if ((newCPK.BaseStream.Position % 0x800) > 0 && i < entries.Count - 1)
                        {
                            long cur_pos = newCPK.BaseStream.Position;
                            for (int j = 0; j < (0x800 - (cur_pos % 0x800)); j++)
                            {
                                newCPK.Write((byte)0);
                            }
                        }

                    }
                    else
                    {
                        string replace_with = batchFileList[currentName.ToString()];
                        //Got patch file name
                        Console.WriteLine("Patching: {0}", currentName.ToString());

                        byte[] newbie = File.ReadAllBytes(replace_with);
                        entries[i].FileOffset = (ulong)newCPK.BaseStream.Position;
                        int o_ext_size = Int32.Parse((entries[i].ExtractSize).ToString());
                        int o_com_size = Int32.Parse((entries[i].FileSize).ToString());
                        if ((o_com_size < o_ext_size) && entries[i].FileType == "FILE" && bUseCompress == true)
                        {
                            // is compressed
                            Console.Write("Compressing data:{0:x8}", newbie.Length);

                            byte[] dest_comp = cpk.CompressCRILAYLA(newbie);

                            entries[i].FileSize = Convert.ChangeType(dest_comp.Length, entries[i].FileSizeType);
                            entries[i].ExtractSize = Convert.ChangeType(newbie.Length, entries[i].FileSizeType);
                            cpk.UpdateFileEntry(entries[i]);
                            newCPK.Write(dest_comp);
                            Console.Write(">> {0:x8}\r\n", dest_comp.Length);
                        }

                        else
                        {
                            Console.Write("Storing data:{0:x8}\r\n", newbie.Length);
                            entries[i].FileSize = Convert.ChangeType(newbie.Length, entries[i].FileSizeType);
                            entries[i].ExtractSize = Convert.ChangeType(newbie.Length, entries[i].FileSizeType);
                            cpk.UpdateFileEntry(entries[i]);
                            newCPK.Write(newbie);
                        }


                        if ((newCPK.BaseStream.Position % 0x800) > 0 && i < entries.Count - 1)
                        {
                            long cur_pos = newCPK.BaseStream.Position;
                            for (int j = 0; j < (0x800 - (cur_pos % 0x800)); j++)
                            {
                                newCPK.Write((byte)0);
                            }
                        }
                    }


                }
                else
                {
                    // Content is special.... just update the position
                    cpk.UpdateFileEntry(entries[i]);
                }
            }

            cpk.WriteCPK(newCPK);
            cpk.WriteITOC(newCPK);
            cpk.WriteTOC(newCPK);
            cpk.WriteETOC(newCPK, cpk.EtocOffset);
            cpk.WriteGTOC(newCPK);

            newCPK.Close();
            oldFile.Close();


        }
        

    }
}
