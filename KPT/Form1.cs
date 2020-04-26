﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

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

        private void EasyPatchButton_Click(object sender, EventArgs e)
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

            folderBrowserDialog1.SelectedPath = "";
            folderBrowserDialog1.Description = "Please Select where you would like the patched iso files to go.";

            folderBrowserDialog1.ShowDialog();

            if (folderBrowserDialog1.SelectedPath == "")
            {
                return;
            }

            string endDirectory = folderBrowserDialog1.SelectedPath;

            folderBrowserDialog1.SelectedPath = "";
            folderBrowserDialog1.Description = "Please select the root directory of the pre-built patch files. (Will have a PATCH.README in it.)";

            folderBrowserDialog1.ShowDialog();

            string patchedFilesDirectory = Path.Combine(folderBrowserDialog1.SelectedPath, "Pre-Built English Files");
            if(folderBrowserDialog1.SelectedPath == "" || !Directory.Exists(patchedFilesDirectory))
            {
                MessageBox.Show("Not Valid Root Directory for Patch Files. ");
                return;
            }

            var isoReader = new ISOReader();
            
            if (!isoReader.OpenISOStream(isoFileName, endDirectory))
            { 
                return; // ISOReader itself should take care of showing a detailed error message
            }

            DirectoryGuard.CheckDirectory(endDirectory);

            List<string> patchedFiles = new List<string>();

            GetAllPrePatchedFiles(patchedFilesDirectory, patchedFiles);
            Dictionary<string, string> outputPatchedFiles = new Dictionary<string, string>();

            patchedFiles.ForEach(patchedFileSource => outputPatchedFiles.Add(Path.Combine(endDirectory, patchedFileSource.Substring(patchedFilesDirectory.Length+1)), patchedFileSource));

            var isoFiles = isoReader.GetGenerator();
            try
            {
                //First We Dump the Iso and make list of files in need of patching.
                foreach (var file in isoFiles)
                {

                    string fileName = Path.Combine(endDirectory, file.name);

                    DirectoryGuard.CheckDirectory(fileName);

                    FileStream fs = new FileStream(fileName, FileMode.Create);


                    if (outputPatchedFiles.ContainsKey(fileName))
                    {
                        FileStream patchedFileStream = new FileStream(outputPatchedFiles[fileName], FileMode.Open);
                        patchedFileStream.CopyTo(fs);
                        patchedFileStream.Close();
                    }
                    else
                    {
                        file.dataStream.CopyTo(fs);
                    }

                    fs.Close();
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
