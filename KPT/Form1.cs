using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;

namespace KPT
{
    public partial class Form1 : Form
    {
        class InitalizationData
        {
            public string lastOpenedProjectFile = "";
        }

        const string lastOpenedFileData = "last.yaml";
        InitalizationData initalizationData = new InitalizationData(); 

        public Form1()
        {
            InitializeComponent();
            this.Text = "KPT";

            if (LoadLastOpened())
            {
                button3.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";

            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName == "")
            {
                return;
            }

            if (ProjectFolder.ReadProjectFile(openFileDialog1.FileName))
            {
                this.Visible = false;

                var projectForm = new ProjectForm();

                ProjectFolder.rootDir = Path.GetDirectoryName(openFileDialog1.FileName);
                
                initalizationData.lastOpenedProjectFile = openFileDialog1.FileName;
                SaveLastOpened();
                LoadLastOpened();

                projectForm.ShowDialog();

                this.Visible = true;
            }

        }

        private void SaveLastOpened()
        {
            FileStream fs = new FileStream("last.yaml", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            var yamlSerialzer = new SharpYaml.Serialization.Serializer();

            var serialzedData = yamlSerialzer.Serialize(initalizationData);

            sw.Write(serialzedData);

            sw.Close();
            fs.Close();

        }

        private bool LoadLastOpened()
        {
            if (!File.Exists(lastOpenedFileData))
            {
                return false;
            }

            FileStream fs = new FileStream(lastOpenedFileData, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            var fileData = sr.ReadToEnd();

            sr.Close();
            fs.Close();

            var yamlSerialzer = new SharpYaml.Serialization.Serializer();
            initalizationData = yamlSerialzer.Deserialize<InitalizationData>(fileData);

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = "";

            folderBrowserDialog1.ShowDialog();

            if (folderBrowserDialog1.SelectedPath == "")
            {
                return;
            }

            string selectedDir = folderBrowserDialog1.SelectedPath;

            if (ProjectFolder.InitalizeNewProjectFolder(selectedDir))
            {
                MessageBox.Show(string.Format("Project file created at {0}!", folderBrowserDialog1.SelectedPath));
            }
            else
            {
                MessageBox.Show("There was an error while attempting to create the new project file.");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {

            string projectFile = initalizationData.lastOpenedProjectFile;

            if (ProjectFolder.ReadProjectFile(projectFile))
            {
                this.Visible = false;

                var projectForm = new ProjectForm();

                ProjectFolder.rootDir = Path.GetDirectoryName(projectFile);

                projectForm.ShowDialog();

                this.Visible = true;
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var about = new About();
            about.ShowDialog();
            
        }

        /*private void EasyPatchButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "ISO|*.iso";
            openFileDialog1.ShowDialog();
            openFileDialog1.Title = "Please Select the Kokoro Connect .iso file.";

            if (openFileDialog1.FileName == "")
            {
                return;
            }

            string isoFileName = openFileDialog1.FileName;


            openFileDialog1.FileName = "EasyPatch.kpt";
            openFileDialog1.Filter = "KPT|*.kpt";
            openFileDialog1.ShowDialog();
            openFileDialog1.Title = "Please Select the Kokoro Connect .kpt patch file.";

            if (openFileDialog1.FileName == "")
            {
                return;
            }

            string patchZip = openFileDialog1.FileName;

            folderBrowserDialog1.SelectedPath = "";
            folderBrowserDialog1.Description = "Please Select where you would like the patched iso files to go.";

            folderBrowserDialog1.ShowDialog();

            if (folderBrowserDialog1.SelectedPath == "")
            {
                return;
            }

            string endDirectory = folderBrowserDialog1.SelectedPath;            

            ZipArchive patchDirectory = ZipFile.OpenRead(patchZip);

            var isoReader = new ISOReader();
            
            if (!isoReader.OpenISOStream(isoFileName, endDirectory))
            { 
                return; // ISOReader itself should take care of showing a detailed error message
            }

            DirectoryGuard.CheckDirectory(endDirectory);

            IEnumerable<ZipArchiveEntry> prebuiltFiles = patchDirectory.Entries.Where(patchEntry => patchEntry.Name.EndsWith(".cpk"));
            
            var isoFiles = isoReader.GetGenerator();
            try
            {
                //First We Dump the Iso and make list of files in need of patching.
                foreach (var file in isoFiles)
                {

                    string fileName = Path.Combine(endDirectory, file.name);

                    DirectoryGuard.CheckDirectory(fileName);

                    var prebuiltFile = prebuiltFiles.SingleOrDefault(fileEntry => file.name.EndsWith(fileEntry.Name));

                    if (prebuiltFile == null)
                    {
                        FileStream fs = new FileStream(fileName, FileMode.Create);
                        file.dataStream.CopyTo(fs);
                        fs.Close();
                    }
                    else
                    {
                        prebuiltFile.ExtractToFile(fileName);
                    }                    
                }

                MessageBox.Show("That was easy.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an issue. Please contact Spud and yell at him to fix things.");
            }
            finally
            {
                isoReader.CloseISOStream();
            }
        }*/

        private void EasyPatchButton_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            var easyPatchForm = new EasyPatch();

            easyPatchForm.ShowDialog();

            this.Visible = true;
        }

        private void GetAllPrePatchedFiles(string patchedFileDirectory, List<string> patchedFiles)
        {
            foreach(string fileName in Directory.GetFiles(patchedFileDirectory))
            {
                patchedFiles.Add(fileName);
            }
            foreach(string directoryName in Directory.GetDirectories(patchedFileDirectory))
            {
                GetAllPrePatchedFiles(directoryName, patchedFiles);
            }
        }        
    }
}
