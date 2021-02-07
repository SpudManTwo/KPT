using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace KPT
{
    public partial class EasyPatch : Form
    {
        public EasyPatch()
        {
            InitializeComponent();
        }

        private void btnIsoPath_Click(object sender, EventArgs e)
        {
            dlgIsoPath.FileName = "";
            dlgIsoPath.Filter = "ISO|*.iso";
            dlgIsoPath.ShowDialog();
            dlgIsoPath.Title = "Please Select the Kokoro Connect .iso file.";

            if (dlgIsoPath.FileName == "")
            {
                return;
            }

            txtIsoPath.Text = dlgIsoPath.FileName;

            ValidateForm(sender, e);
        }

        private void btnPatchPath_Click(object sender, EventArgs e)
        {
            dlgPatchPath.FileName = "EasyPatch.kpt";
            dlgPatchPath.Filter = "KPT|*.kpt";
            dlgPatchPath.ShowDialog();
            dlgPatchPath.Title = "Please Select the Kokoro Connect .kpt patch file.";

            if (dlgPatchPath.FileName == "")
            {
                return;
            }

            txtPatchPath.Text = dlgPatchPath.FileName;

            ValidateForm(sender, e);
        }

        private void btnOutputPath_Click(object sender, EventArgs e)
        {
            dlgOutputPath.FileName = "";
            dlgOutputPath.Filter = "ISO|*.iso";
            dlgOutputPath.Title = "Please Select where you would like the patched iso file to go.";
            dlgOutputPath.ShowDialog();

            if (dlgOutputPath.FileName == "")
            {
                return;
            }

            txtOutputPath.Text = dlgOutputPath.FileName;

            ValidateForm(sender, e);
        }

        private void ValidateForm(object sender, EventArgs e)
        {
            if(txtIsoPath.Text == "" || !File.Exists(txtIsoPath.Text))
            {
                btnPatch.Enabled = false;
                return;
            }

            if (txtOutputPath.Text == "")
            {
                btnPatch.Enabled = false;
                return;
            }

            if (txtPatchPath.Enabled && (txtPatchPath.Text == "" || !File.Exists(txtPatchPath.Text)))
            {
                btnPatch.Enabled = false;
                return;
            }

            btnPatch.Enabled = true;
            this.patchTask = null;
        }

        private void btnPatch_Click(object sender, EventArgs e)
        {
            //Disable the changes during the patch process
            txtIsoPath.Enabled = false;
            btnIsoPath.Enabled = false;
            txtPatchPath.Enabled = false;
            btnPatchPath.Enabled = false;
            txtOutputPath.Enabled = false;
            btnOutputPath.Enabled = false;
            btnPatch.Enabled = false;
            if (this.patchTask == null)
            {
                logFilePath = txtOutputPath.Text + Path.PathSeparator + "log.txt";
                this.patchTask = PatchGame();
            }
        }

        private async Task PatchGame()
        {
            ZipArchive patchDirectory = null;
            if (txtPatchPath.Enabled)
            {
                patchDirectory = ZipFile.OpenRead(txtPatchPath.Text);
            }
            else
            {
                patchDirectory = ZipFile.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "Libraries\\Patch\\PrePatchedFiles.kpt");
            }

            if (File.Exists(txtOutputPath.Text))
            {
                File.Delete(txtOutputPath.Text);
            }

            File.Copy(txtIsoPath.Text, txtOutputPath.Text);

            txtPatchOutputProcess.AppendText("Enumerating Patch Files...\n");
            txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
            txtPatchOutputProcess.ScrollToCaret();
            IEnumerable<ZipArchiveEntry> prebuiltFiles = patchDirectory.Entries.Where(patchEntry => patchEntry.Name.EndsWith(".cpk") || patchEntry.Name.EndsWith(".SFO") || patchEntry.Name.EndsWith(".BIN") || patchEntry.Name.EndsWith(".PNG"));
            txtPatchOutputProcess.AppendText($"{prebuiltFiles.Count()} + files found.\n");
            txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
            txtPatchOutputProcess.ScrollToCaret();

            try
            {
                filesProcessed = 0;
                totalFiles = (double)prebuiltFiles.Count();
                //First Argument is iso location
                //Second Argument is going to be Batch Arguments temp file.

                List<string> batchArgs = new List<string>();
                List<string> tempFilePaths = new List<string>();
                txtPatchOutputProcess.AppendText("Setting up Temporary Patch Storage...\n");
                txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                txtPatchOutputProcess.ScrollToCaret();

                //Wipe out any previous data that somehow persisted.
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Libraries\\UMD-ReplaceK\\TemporaryPatchStorage"))
                {
                    txtPatchOutputProcess.AppendText("Deleting Previous Temporary Patch Storage...\n");
                    txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                    txtPatchOutputProcess.ScrollToCaret();

                    Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + "Libraries\\UMD-ReplaceK\\TemporaryPatchStorage", true);
                }
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Libraries\\UMD-ReplaceK\\TemporaryPatchStorage");
                txtPatchOutputProcess.AppendText("Patch Storage is good to go.\n");
                txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                txtPatchOutputProcess.ScrollToCaret();

                foreach (ZipArchiveEntry patchFile in prebuiltFiles)
                {
                    //Generate a temp file path
                    var tempFilePath = AppDomain.CurrentDomain.BaseDirectory + "Libraries\\UMD-ReplaceK\\TemporaryPatchStorage\\" + patchFile.FullName.Substring(patchFile.FullName.IndexOf("PSP_GAME")).Replace("/", "\\");
                    
                    //Create directories as needed.
                    if(!Directory.Exists(Directory.GetParent(tempFilePath).FullName))
                    {
                        Directory.CreateDirectory(Directory.GetParent(tempFilePath).FullName);
                    }

                    txtPatchOutputProcess.AppendText($"Extracting Patch File: {patchFile.FullName}\n");
                    txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                    txtPatchOutputProcess.ScrollToCaret();

                    //Extract the patched file to the temp file.
                    patchFile.ExtractToFile(tempFilePath);

                    //Add File to Arguments
                    batchArgs.Add(patchFile.FullName.Substring(patchFile.FullName.IndexOf("PSP_GAME")).Replace("/", "\\"));
                    batchArgs.Add("TemporaryPatchStorage\\" + patchFile.FullName.Substring(patchFile.FullName.IndexOf("PSP_GAME")).Replace("/", "\\"));
                    tempFilePaths.Add(tempFilePath);

                    filesProcessed += 0.1;
                    //Update Progress Bar with 1/3rd since extraction is the first third of progress.
                    PatchProgressBar.Value = (int)((filesProcessed / totalFiles) * 100);
                }

                //Generate a temp file path
                string batchArgsFilePath = Path.GetTempFileName();
                //Delete the auto generated file from calling the GetTempFileName
                txtPatchOutputProcess.AppendText("Building batch patch file.\n");
                txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                txtPatchOutputProcess.ScrollToCaret();

                using (StreamWriter batchArgsFileWriter = new StreamWriter(new FileStream(batchArgsFilePath, FileMode.Open)))
                {
                    batchArgs.ForEach(batchArg => batchArgsFileWriter.WriteLine(batchArg));
                }
                txtPatchOutputProcess.AppendText("Batch Patch File is good to go.\n");
                txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                txtPatchOutputProcess.ScrollToCaret();

                //Run UMD-ReplaceK to replace the file.
                txtPatchOutputProcess.AppendText("Setting up UMD-ReplaceK Patch Process\n");
                txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                txtPatchOutputProcess.ScrollToCaret();

                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + "Libraries\\UMD-ReplaceK\\";
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "Libraries\\UMD-ReplaceK\\UMDReplaceK.exe";
                processStartInfo.Arguments = "\"" + txtOutputPath.Text + "\" "+batchArgsFilePath;
                processStartInfo.RedirectStandardOutput = true;
                Process replaceProcess = null;

                txtPatchOutputProcess.AppendText("UMD-ReplaceK Patch Process is good to go.\n");
                txtPatchOutputProcess.AppendText("All systems are go.\n");
                txtPatchOutputProcess.AppendText("All lights are green.\n");
                txtPatchOutputProcess.AppendText("Houston, we have patching.\n");
                txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                txtPatchOutputProcess.ScrollToCaret();


                replaceProcess = Process.Start(processStartInfo);
                while(!replaceProcess.HasExited)
                {
                    string outputLine = replaceProcess.StandardOutput.ReadLine() + "\n";
                    txtPatchOutputProcess.AppendText(outputLine);
                    txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                    txtPatchOutputProcess.ScrollToCaret();

                    if(outputLine.Contains("- replacing file "))
                    {
                        //Increment Patch Progress Bar.
                        filesProcessed += 0.8;
                        PatchProgressBar.Value = (int)((filesProcessed / totalFiles) * 100);
                    }
                }

                PatchProgressBar.Value = (int)((filesProcessed / totalFiles) * 100);
                //If things broke, throw exception
                if (replaceProcess.ExitCode != 0)
                {
                    txtPatchOutputProcess.AppendText("Houston, we have a problem.\n");
                    txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                    txtPatchOutputProcess.ScrollToCaret();

                    throw new Exception(replaceProcess.StandardOutput.ReadToEnd());
                }

                txtPatchOutputProcess.AppendText("Performance nominal.\n");
                txtPatchOutputProcess.AppendText("We have a go for second stage.\n");
                txtPatchOutputProcess.AppendText("Cleaning up the mess of files we created...\n");
                txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                txtPatchOutputProcess.ScrollToCaret();

                //Update progress bar to show how close we are to completion.
                filesProcessed = tempFilePaths.Count() * 0.9;
                PatchProgressBar.Value = (int)((filesProcessed / totalFiles) * 100);

                //Delete the temp files once we are done
                tempFilePaths.ForEach(tempFilePath =>
                {
                    txtPatchOutputProcess.AppendText($"Deleting temp patch file: {tempFilePath}\n");
                    txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                    txtPatchOutputProcess.ScrollToCaret();

                    File.Delete(tempFilePath);
                    //Update progress bar with each temp file deletion.
                    filesProcessed += 0.1;
                    PatchProgressBar.Value = (int)((filesProcessed / totalFiles) * 100);
                });

                txtPatchOutputProcess.AppendText("Deploying landing gear.\n");
                txtPatchOutputProcess.SelectionStart = txtPatchOutputProcess.Text.Length;
                txtPatchOutputProcess.ScrollToCaret();

                File.Delete(batchArgsFilePath);

                MessageBox.Show("That was easy.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an issue. Please send the log located at " + logFilePath + " to SpudManTwo and yell at him to fix things.");
                using (System.IO.StreamWriter logFile = new System.IO.StreamWriter(logFilePath, true))
                {
                    logFile.WriteLine("Exception hit:\n" + ex.ToString());
                }
            }
            finally
            {
                patchDirectory.Dispose();

                //Re-Enable text boxes
                txtIsoPath.Enabled = true;
                btnIsoPath.Enabled = true;
                txtPatchPath.Enabled = true;
                btnPatchPath.Enabled = true;
                txtOutputPath.Enabled = true;
                btnOutputPath.Enabled = true;

                System.Timers.Timer renablePatchTier = new System.Timers.Timer(2000);
                renablePatchTier.Elapsed += ReenablePatchButton;
                renablePatchTier.AutoReset = false;
                renablePatchTier.Enabled = true;
            }

            Close();
        }

        private async Task PatchGameDeprecated()
        {
            ZipArchive patchDirectory = ZipFile.OpenRead(txtPatchPath.Text);

            var isoReader = new ISOReader();

            if (!isoReader.OpenISOStream(txtIsoPath.Text, txtOutputPath.Text))
            {
                return; // ISOReader itself should take care of showing a detailed error message
            }

            DirectoryGuard.CheckDirectory(txtOutputPath.Text);

            IEnumerable<ZipArchiveEntry> prebuiltFiles = patchDirectory.Entries.Where(patchEntry => patchEntry.Name.EndsWith(".cpk") || patchEntry.Name.EndsWith(".SFO"));

            var isoFiles = isoReader.GetGenerator();

            try
            {
                filesProcessed = 0;
                totalFiles = (double)isoFiles.Count();

                //First We Dump the Iso and make list of files in need of patching.

                List<Task> tasks = isoFiles.Select(file => PatchFileDeprecated(file, prebuiltFiles)).ToList();

                await Task.WhenAll(tasks);

                MessageBox.Show("That was easy.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an issue. Please send the log located at "+logFilePath+" to Spud and yell at him to fix things.");
                using (System.IO.StreamWriter logFile = new System.IO.StreamWriter(logFilePath, true))
                {
                    logFile.WriteLine("Exception hit:\n" + ex.ToString());
                }
            }
            finally
            {
                isoReader.CloseISOStream();

                //Re-Enable text boxes
                txtIsoPath.Enabled = true;
                btnIsoPath.Enabled = true;
                txtPatchPath.Enabled = true;
                btnPatchPath.Enabled = true;
                txtOutputPath.Enabled = true;
                btnOutputPath.Enabled = true;

                System.Timers.Timer renablePatchTier = new System.Timers.Timer(2000);
                renablePatchTier.Elapsed += ReenablePatchButton;
                renablePatchTier.AutoReset = false;
                renablePatchTier.Enabled = true;
            }
        }

        private void ReenablePatchButton(Object source, ElapsedEventArgs e)
        {
            btnPatch.Enabled = true;
        }

        private async Task PatchFileDeprecated(EmbeddedFileAccessor file, IEnumerable<ZipArchiveEntry> prebuiltFiles)
        {
            using (System.IO.StreamWriter logFile = new System.IO.StreamWriter(logFilePath, true))
            {
                logFile.WriteLine("Beginning work on file: "+file.name);
            }

            string fileName = Path.Combine(txtOutputPath.Text, file.name);

            DirectoryGuard.CheckDirectory(fileName);

            var prebuiltFile = prebuiltFiles.SingleOrDefault(fileEntry => String.Equals(fileEntry.FullName.Substring(prebuiltFiles.First().FullName.IndexOf("PSP_GAME")),file.name.Replace("\\","/"), StringComparison.OrdinalIgnoreCase));

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

            using (System.IO.StreamWriter logFile = new System.IO.StreamWriter(logFilePath, true))
            {
                logFile.WriteLine("Completed work on file: " + file.name);
            }

            this.filesProcessed++;

            //Update Progress Bar
            PatchProgressBar.Value = (int)((this.filesProcessed / this.totalFiles) * 100);
        }

        private void EasyPatch_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            lblPatchPath.Visible = !lblPatchPath.Visible;
            lblPatchPath.Enabled = !lblPatchPath.Enabled;
            txtPatchPath.Visible = !txtPatchPath.Visible;
            txtPatchPath.Enabled = !txtPatchPath.Enabled;
            btnPatchPath.Visible = !btnPatchPath.Visible;
            btnPatchPath.Enabled = !btnPatchPath.Enabled;
        }
    }
}
