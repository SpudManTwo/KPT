using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.BuildObjects;
using System.Windows.Forms;
using KPT.Parser;
using KPT.Parser.Elements;
using KPT.Parser.Instructions;
using KPT.Parser.Spreadsheet_Interface;
using KPT.Parser.Jump_Label_Manager;
using System.ComponentModel;
using System.Threading;

namespace KPT
{
    // this is just a shell of a class at the moment that will become an important reference point for the UI later - it is possible that the directory structure from the Dumper will be moved here as well
    static class ProjectFolder
    {

        class ProjectFile
        {

        }

        /// <summary>
        /// The location of the project / the root directory from which the paths of all other directories will be derived
        /// </summary>
        public static string rootDir = @"H:\kokoro_connect\BuildingTest";
        /// <summary>
        /// Dump of ISO contents - CPKs etc.
        /// </summary>
        public const string extractedISODir = "Extracted ISO Files";
        /// <summary>
        /// Game files extracted from CPKs in extracted ISO folder or standalone files copied over directly from the extracted ISO folder - bin files etc.
        /// </summary>
        public const string unpackedGameFilesDir = "Unpacked Game Files";
        /// <summary>
        /// Disassembled game files
        /// </summary>
        public const string disassemblyDir = "Disassembled Files";
        /// <summary>
        /// Files presented in a more readily editable format - spreadsheets of strings etc.
        /// </summary>
        public const string editableGameFiesDir = "Editable Game Files";
        /// <summary>
        /// Game files that have been reassembled - bins etc.
        /// </summary>
        public const string reassembledGameFilesDir = "Reassembled Game Files";
        /// <summary>
        /// Game files that have been repacked where necessary - CPKs etc.
        /// </summary>
        public const string repackedGameFilesDir = "Repacked Game Files";
        /// <summary>
        /// XML files, scripts, etc. used to direct the reassembly or repacking of files
        /// </summary>
        public const string buildScriptsDir = "Build Scripts";
        /// <summary>
        /// The working project file
        /// </summary>
        private static ProjectFile projectFile = new ProjectFile();
        /// <summary>
        /// The file name to which the project file will be saved
        /// </summary>
        private const string projectFileName = "project.yaml";
        /// <summary>
        /// Directory in which tools such as mkisofs or GimConv will be stored
        /// </summary>
        public const string toolsDir = "./Tools";

        public static string GetRootDir()
        {
            return rootDir;
        }

        public static bool CheckProjectFile(string fileName)
        {
            return true;
        }

        public static void RebuildCPKs(object sender, EventArgs e)
        {

            BackgroundWorker worker = null;
            DoWorkEventArgs eventArgs = null;

            if (sender is BackgroundWorker)
            {
                worker = sender as BackgroundWorker;
                eventArgs = e as DoWorkEventArgs;
            }
            else
            {
                worker = null;
            }

            string buildDir = Path.Combine(rootDir, buildScriptsDir);
            
            if (!Directory.Exists(buildDir))
            {
                string errorMessage = string.Format("Could not find build scripts directory at {0}.", buildDir);
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                eventArgs.Result = false;
            }

            string[] fileList = Directory.GetFiles(buildDir);

            int counter = 0;

            foreach (var file in fileList)
            {
                counter++;

                if (worker != null)
                {
                    double progress = ((double)counter / (double)fileList.Length) * 100;

                    worker.ReportProgress((int)progress);

                    Thread.Sleep(3); // these are aeshetic sleeps, there to make the progress bar grow steadily so the user doesn't think something might have went wrong
                }

                if (worker != null)
                {
                    if (worker.WorkerSupportsCancellation && worker.CancellationPending)
                    {
                        eventArgs.Result = false;
                        MessageBox.Show("Rebuild CPKs cancelled");
                        return;
                    }
                }

                if (file.EndsWith(".cpk.yaml"))
                {
                    if (file.EndsWith("movie-movie.cpk.yaml")) // doesn't work for some reason - manually blacklisting
                    {
                        continue;
                    }
                    CPKBuildObject cpk = new CPKBuildObject();
                    if (!cpk.BuildCPK(file))
                    {
                        eventArgs.Result = false;
                    }
                }
            }

            Thread.Sleep(1000); // this is another aesthetic sleep, ensuring the the progress bar does not disappear before the user can see it completing

            using (System.IO.StreamWriter logFile = new System.IO.StreamWriter(buildDir+Path.DirectorySeparatorChar+"build.log", true))
            {
                CPKBuildObject.changedCPKs.ForEach(changedCPK => logFile.WriteLine(changedCPK));
            }

            eventArgs.Result = true;
        }

        public static bool DumpStrings()
        {

            string dialogueFileDir = Path.Combine(rootDir, unpackedGameFilesDir, @"PSP_GAME\USRDIR\St000"); // at the moment there's only one directory with files that have strings and only files with strings in that directory, so this is a quick way of processing. if any non-string files are added to this directory or any string files are found in other directories more advanced filtering will be necessary.
            string[] fileList = Directory.GetFiles(dialogueFileDir);

            if (!Directory.Exists(dialogueFileDir))
            {
                string errorMessage = string.Format("Could not find directory {0}.", dialogueFileDir);
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var nameCollection = new StringCollection();
            Dictionary<string, StringCollection> stringFiles = new Dictionary<string, StringCollection>();

            foreach (string file in fileList)
            {
                string fileName = Path.Combine(dialogueFileDir, file);
                var fileStrings = new StringCollection(GetSubPath(file, Path.Combine(rootDir, unpackedGameFilesDir)));                

                FileStream fs = new FileStream(fileName, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);

                var testParser = new FileParser();
                var parsedFile = testParser.ParseFile(br, Path.GetFileName(file), null);

                foreach (IElement element in parsedFile.instructions)
                {
                    if (element is IHasName)
                    {
                        var name = (element as IHasName).GetName();
                        nameCollection.AddString(name, name); // using name as ID and value since we want a single entry for each name - saves messing with IDs
                    }
                    if (element is IHasStrings)
                    {
                        var temp = element as IHasStrings;
                        temp.AddStrings(fileStrings);
                    }
                }

                br.Close();
                fs.Close();
                
                if (fileStrings.NumberOfKeys > 0)
                {
                    var subPath = GetSubPath(file, Path.Combine(rootDir, unpackedGameFilesDir));
                    string spreadsheetFileName = Path.Combine(rootDir, editableGameFiesDir, subPath);
                    var spreadsheet = new CSVFileWriter();
                    spreadsheet.WriteCSVFile(spreadsheetFileName, parsedFile.instructions, fileStrings);
                }

            }

            return true;
        }

        // under ideal circumstances a large portion of DumpStrings and LoadStrings code would be seperated out into ProcessStrings - but that's more of goal for future refactoring
        // yeah this is just getting done to provide a quick beta and will have to be refactored later
        public static void LoadStrings(object sender, EventArgs e)
        {

            BackgroundWorker worker = null;
            DoWorkEventArgs eventArgs = null;

            if (sender is BackgroundWorker)
            {
                worker = sender as BackgroundWorker;
                eventArgs = e as DoWorkEventArgs;
            }
            else
            {
                worker = null;
            }

            string dialogueFileDir = Path.Combine(rootDir, unpackedGameFilesDir, @"PSP_GAME\USRDIR\St000"); // at the moment there's only one directory with files that have strings and only files with strings in that directory, so this is a quick way of processing. if any non-string files are added to this directory or any string files are found in other directories more advanced filtering will be necessary.
            string[] fileList = Directory.GetFiles(dialogueFileDir);
            StringCollection translatedStrings = LoadCSVStrings(Path.Combine(rootDir, editableGameFiesDir, @"PSP_GAME\USRDIR\St000"));

            if (!Directory.Exists(dialogueFileDir))
            {
                string errorMessage = string.Format("Could not find directory {0}.", dialogueFileDir);
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                eventArgs.Result = false;
            }

            var nameCollection = new StringCollection();
            Dictionary<string, StringCollection> stringFiles = new Dictionary<string, StringCollection>();

            FontHandler fontHandler = new FontHandler(); // we will sneak fixing the font size in here
            fontHandler.ChangeSpaceSize(FontHandler.DEFAULT_SPACE_SIZE);

            var textWrapper = new DynamicTextBoxes();
            var jumpTable = new KPT.Parser.Jump_Label_Manager.JumpTableInterface();
            jumpTable.LoadJumpTable(); // changing the size of instructions with new dialogue will mess up file offsets so we will need to load the jump table and make sure it's updated as we go
            var jumpLabelManager = new JumpLabelManager(jumpTable.GetJumpTableEntries());

            int counter = 0;

            foreach (string file in fileList)
            {

                counter++;

                if (worker != null)
                {
                    double progress = ((double)counter / (double)fileList.Length) * 100;

                    worker.ReportProgress((int)progress);

                    Thread.Sleep(3); // these are aeshetic sleeps, there to make the progress bar grow steadily so the user doesn't think something might have went wrong
                }

                if (worker != null)
                {
                    if (worker.WorkerSupportsCancellation && worker.CancellationPending)
                    {
                        eventArgs.Result = false;
                        MessageBox.Show("Load strings cancelled");
                        return;
                    }
                }

                string fileName = Path.Combine(dialogueFileDir, file);
                var fileStrings = new StringCollection(GetSubPath(file, Path.Combine(rootDir, unpackedGameFilesDir)));

                FileStream fs = new FileStream(fileName, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);

                var parser = new FileParser();
                var parsedFile = parser.ParseFile(br, Path.GetFileName(file), jumpLabelManager);

                foreach (IElement element in parsedFile.instructions)
                {
                    if (element is IHasName)
                    {
                        var name = (element as IHasName).GetName();
                        nameCollection.AddString(name, name); // using name as ID and value since we want a single entry for each name - saves messing with IDs
                    }
                    if (element is IHasStrings)
                    {
                        var temp = element as IHasStrings;
                        temp.AddStrings(fileStrings);
                        temp.GetStrings(translatedStrings);
                    }
                }

                textWrapper.CheckTextboxes(parsedFile);

                br.Close();
                fs.Close();

                if (fileStrings.NumberOfKeys > 0)
                {
                    string outputFileName = Path.Combine(rootDir, reassembledGameFilesDir, GetSubPath(file, Path.Combine(rootDir, unpackedGameFilesDir)));
                    parser.WriteFile(parsedFile, outputFileName);
                }                

            }

            jumpTable.SaveJumpTable();

            Thread.Sleep(1000); // this is another aesthetic sleep, ensuring the the progress bar does not disappear before the user can see it completing
            eventArgs.Result = true;
        }

        public static bool DumpISO(string isoFileName)
        {
            var isoReader = new ISOReader();
            string isoOutputDir = Path.Combine(rootDir, extractedISODir);

            if (!isoReader.OpenISOStream(isoFileName, isoOutputDir))
            { 
                return false; // ISOReader itself should take care of showing a detailed error message
            }

            DirectoryGuard.CheckDirectory(isoOutputDir);

            var isoFiles = isoReader.GetGenerator();

            foreach (var file in isoFiles)
            {
                string fileName = Path.Combine(isoOutputDir, file.name);

                DirectoryGuard.CheckDirectory(fileName);

                FileStream fs = new FileStream(fileName, FileMode.Create);

                file.dataStream.CopyTo(fs);

                fs.Close();
            }

            isoReader.CloseISOStream();

            return true;
        }

        private static StringCollection LoadCSVStrings(string directory)
        {
            StringCollection collection = new StringCollection();
            string[] fileList = Directory.GetFiles(directory); // may need to be made recursive/able to look through child directories when more string directories are added
            var csvReader = new CSVFileReader();

            foreach (var file in fileList)
            {
                if (file.EndsWith(".csv"))
                {
                    csvReader.ReadCSVFile(file, collection);
                }
            }

            return collection;
        }

        public static string GetSubPath(string path, string root)
        {
            int rootLen = root.Length + Path.DirectorySeparatorChar.ToString().Length;
            int subPathLen = path.Length - rootLen;
            return path.Substring(rootLen, subPathLen);
        }

        public static bool InitalizeNewProjectFolder(string directory)
        {

            rootDir = directory;

            try
            {
                Directory.CreateDirectory(Path.Combine(rootDir, extractedISODir));
                Directory.CreateDirectory(Path.Combine(rootDir, unpackedGameFilesDir));
                Directory.CreateDirectory(Path.Combine(rootDir, disassemblyDir));
                Directory.CreateDirectory(Path.Combine(rootDir, editableGameFiesDir));
                Directory.CreateDirectory(Path.Combine(rootDir, reassembledGameFilesDir));
                Directory.CreateDirectory(Path.Combine(rootDir, repackedGameFilesDir));
                Directory.CreateDirectory(Path.Combine(rootDir, buildScriptsDir));
                WriteProjectFile();
            }
            catch (Exception e)
            {
                string errorMessage = string.Format("There was an error while initalizing the project directory.\r\n\r\n{0}", e.Message);
                MessageBox.Show(errorMessage);
                return false;
            }

            return true;
        }

        private static void WriteProjectFile()
        {
            string fileName = Path.Combine(rootDir, projectFileName);

            FileStream fs = new FileStream(fileName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            var yamlSerialzer = new SharpYaml.Serialization.Serializer();

            var serialzedData = yamlSerialzer.Serialize(projectFile);

            sw.Write(serialzedData);

            sw.Close();
            fs.Close();
        }

        public static bool ReadProjectFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }

            FileStream fs = new FileStream(fileName, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            var fileData = sr.ReadToEnd();

            sr.Close();
            fs.Close();

            try
            {
                var yamlSerialzer = new SharpYaml.Serialization.Serializer();
                projectFile = yamlSerialzer.Deserialize<ProjectFile>(fileData);
            }
            catch (Exception e)
            {
                string errorMessage = string.Format("There was an error while reading the project file.\r\n\r\n{0}", e.Message);
                MessageBox.Show(errorMessage);
                return false;
            }

            ImageHandler.initalized = false; // necessary for if the rootDir changes, a bit weird to struture the program this way. think about changing it. (update: just found a weird bug related to ImageHandler initialization, definitely needs changed)

            return true;
        }

    }
}
