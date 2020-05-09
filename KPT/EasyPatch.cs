using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
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
            dlgOutputPath.SelectedPath = "";
            dlgOutputPath.Description = "Please Select where you would like the patched iso files to go.";

            dlgOutputPath.ShowDialog();

            if (dlgOutputPath.SelectedPath == "")
            {
                return;
            }

            txtOutputPath.Text = dlgOutputPath.SelectedPath;

            ValidateForm(sender, e);
        }

        private void ValidateForm(object sender, EventArgs e)
        {
            if(txtIsoPath.Text == "" || !File.Exists(txtIsoPath.Text))
            {
                btnPatch.Enabled = false;
                return;
            }

            if (txtPatchPath.Text == "" || !File.Exists(txtPatchPath.Text))
            {
                btnPatch.Enabled = false;
                return;
            }

            if (txtOutputPath.Text == "" || !Directory.Exists(txtOutputPath.Text))
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
            ZipArchive patchDirectory = ZipFile.OpenRead(txtPatchPath.Text);

            var isoReader = new ISOReader();

            if (!isoReader.OpenISOStream(txtIsoPath.Text, txtOutputPath.Text))
            {
                return; // ISOReader itself should take care of showing a detailed error message
            }

            DirectoryGuard.CheckDirectory(txtOutputPath.Text);

            IEnumerable<ZipArchiveEntry> prebuiltFiles = patchDirectory.Entries.Where(patchEntry => patchEntry.Name.EndsWith(".cpk"));

            var isoFiles = isoReader.GetGenerator();

            try
            {
                filesProcessed = 0;
                totalFiles = (double)isoFiles.Count();

                //First We Dump the Iso and make list of files in need of patching.

                List<Task> tasks = isoFiles.Select(file => PatchFile(file, prebuiltFiles)).ToList();

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

        private async Task PatchFile(EmbeddedFileAccessor file, IEnumerable<ZipArchiveEntry> prebuiltFiles)
        {
            using (System.IO.StreamWriter logFile = new System.IO.StreamWriter(logFilePath, true))
            {
                logFile.WriteLine("Beginning work on file: "+file.name);
            }

            string fileName = Path.Combine(txtOutputPath.Text, file.name);

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

            using (System.IO.StreamWriter logFile = new System.IO.StreamWriter(logFilePath, true))
            {
                logFile.WriteLine("Completed work on file: " + file.name);
            }

            this.filesProcessed++;

            //Update Progress Bar
            PatchProgressBar.Value = (int)((this.filesProcessed / this.totalFiles) * 100);
        }
    }
}
