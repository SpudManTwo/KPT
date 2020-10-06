using System.Threading.Tasks;

namespace KPT
{
    partial class EasyPatch
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtIsoPath = new System.Windows.Forms.TextBox();
            this.lblIsoPath = new System.Windows.Forms.Label();
            this.btnIsoPath = new System.Windows.Forms.Button();
            this.lblInput = new System.Windows.Forms.Label();
            this.PatchProgressBar = new System.Windows.Forms.ProgressBar();
            this.btnPatchPath = new System.Windows.Forms.Button();
            this.lblPatchPath = new System.Windows.Forms.Label();
            this.txtPatchPath = new System.Windows.Forms.TextBox();
            this.lblOutput = new System.Windows.Forms.Label();
            this.btnOutputPath = new System.Windows.Forms.Button();
            this.lblOutputFile = new System.Windows.Forms.Label();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.dlgIsoPath = new System.Windows.Forms.OpenFileDialog();
            this.btnPatch = new System.Windows.Forms.Button();
            this.dlgPatchPath = new System.Windows.Forms.OpenFileDialog();
            this.dlgOutputPath = new System.Windows.Forms.SaveFileDialog();
            this.txtPatchOutputProcess = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtIsoPath
            // 
            this.txtIsoPath.Location = new System.Drawing.Point(233, 84);
            this.txtIsoPath.Name = "txtIsoPath";
            this.txtIsoPath.Size = new System.Drawing.Size(429, 20);
            this.txtIsoPath.TabIndex = 0;
            this.txtIsoPath.TextChanged += new System.EventHandler(this.ValidateForm);
            // 
            // lblIsoPath
            // 
            this.lblIsoPath.AutoSize = true;
            this.lblIsoPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIsoPath.Location = new System.Drawing.Point(12, 85);
            this.lblIsoPath.Name = "lblIsoPath";
            this.lblIsoPath.Size = new System.Drawing.Size(178, 16);
            this.lblIsoPath.TabIndex = 1;
            this.lblIsoPath.Text = "Path To Kokoro Connect ISO";
            // 
            // btnIsoPath
            // 
            this.btnIsoPath.Location = new System.Drawing.Point(686, 81);
            this.btnIsoPath.Name = "btnIsoPath";
            this.btnIsoPath.Size = new System.Drawing.Size(88, 23);
            this.btnIsoPath.TabIndex = 1;
            this.btnIsoPath.Text = "Select ISO";
            this.btnIsoPath.UseVisualStyleBackColor = true;
            this.btnIsoPath.Click += new System.EventHandler(this.btnIsoPath_Click);
            // 
            // lblInput
            // 
            this.lblInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInput.Location = new System.Drawing.Point(9, 18);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(765, 36);
            this.lblInput.TabIndex = 3;
            this.lblInput.Text = "Input                                                                            " +
    "                    ";
            // 
            // PatchProgressBar
            // 
            this.PatchProgressBar.Location = new System.Drawing.Point(15, 583);
            this.PatchProgressBar.Name = "PatchProgressBar";
            this.PatchProgressBar.Size = new System.Drawing.Size(647, 23);
            this.PatchProgressBar.TabIndex = 4;
            // 
            // btnPatchPath
            // 
            this.btnPatchPath.Location = new System.Drawing.Point(686, 147);
            this.btnPatchPath.Name = "btnPatchPath";
            this.btnPatchPath.Size = new System.Drawing.Size(88, 23);
            this.btnPatchPath.TabIndex = 4;
            this.btnPatchPath.Text = "Select KPT";
            this.btnPatchPath.UseVisualStyleBackColor = true;
            this.btnPatchPath.Click += new System.EventHandler(this.btnPatchPath_Click);
            // 
            // lblPatchPath
            // 
            this.lblPatchPath.AutoSize = true;
            this.lblPatchPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPatchPath.Location = new System.Drawing.Point(12, 151);
            this.lblPatchPath.Name = "lblPatchPath";
            this.lblPatchPath.Size = new System.Drawing.Size(215, 16);
            this.lblPatchPath.TabIndex = 6;
            this.lblPatchPath.Text = "Path To Kokoro Connect Patch File";
            // 
            // txtPatchPath
            // 
            this.txtPatchPath.Location = new System.Drawing.Point(233, 150);
            this.txtPatchPath.Name = "txtPatchPath";
            this.txtPatchPath.Size = new System.Drawing.Size(429, 20);
            this.txtPatchPath.TabIndex = 3;
            this.txtPatchPath.TextChanged += new System.EventHandler(this.ValidateForm);
            // 
            // lblOutput
            // 
            this.lblOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOutput.Location = new System.Drawing.Point(9, 201);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(765, 36);
            this.lblOutput.TabIndex = 8;
            this.lblOutput.Text = "Output                                                                           " +
    "                     ";
            // 
            // btnOutputPath
            // 
            this.btnOutputPath.Location = new System.Drawing.Point(686, 267);
            this.btnOutputPath.Name = "btnOutputPath";
            this.btnOutputPath.Size = new System.Drawing.Size(88, 23);
            this.btnOutputPath.TabIndex = 6;
            this.btnOutputPath.Text = "Save To";
            this.btnOutputPath.UseVisualStyleBackColor = true;
            this.btnOutputPath.Click += new System.EventHandler(this.btnOutputPath_Click);
            // 
            // lblOutputFile
            // 
            this.lblOutputFile.AutoSize = true;
            this.lblOutputFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOutputFile.Location = new System.Drawing.Point(12, 271);
            this.lblOutputFile.Name = "lblOutputFile";
            this.lblOutputFile.Size = new System.Drawing.Size(101, 16);
            this.lblOutputFile.TabIndex = 10;
            this.lblOutputFile.Text = "Output ISO Path";
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Location = new System.Drawing.Point(233, 270);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.Size = new System.Drawing.Size(429, 20);
            this.txtOutputPath.TabIndex = 5;
            this.txtOutputPath.TextChanged += new System.EventHandler(this.ValidateForm);
            // 
            // btnPatch
            // 
            this.btnPatch.Enabled = false;
            this.btnPatch.Location = new System.Drawing.Point(686, 583);
            this.btnPatch.Name = "btnPatch";
            this.btnPatch.Size = new System.Drawing.Size(88, 23);
            this.btnPatch.TabIndex = 7;
            this.btnPatch.Text = "Patch Game";
            this.btnPatch.UseVisualStyleBackColor = true;
            this.btnPatch.Click += new System.EventHandler(this.btnPatch_Click);
            // 
            // txtPatchOutputProcess
            // 
            this.txtPatchOutputProcess.Location = new System.Drawing.Point(12, 302);
            this.txtPatchOutputProcess.Name = "txtPatchOutputProcess";
            this.txtPatchOutputProcess.ReadOnly = true;
            this.txtPatchOutputProcess.Size = new System.Drawing.Size(762, 259);
            this.txtPatchOutputProcess.TabIndex = 11;
            this.txtPatchOutputProcess.Text = "";
            // 
            // EasyPatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 618);
            this.Controls.Add(this.txtPatchOutputProcess);
            this.Controls.Add(this.btnPatch);
            this.Controls.Add(this.btnOutputPath);
            this.Controls.Add(this.lblOutputFile);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.btnPatchPath);
            this.Controls.Add(this.lblPatchPath);
            this.Controls.Add(this.txtPatchPath);
            this.Controls.Add(this.PatchProgressBar);
            this.Controls.Add(this.lblInput);
            this.Controls.Add(this.btnIsoPath);
            this.Controls.Add(this.lblIsoPath);
            this.Controls.Add(this.txtIsoPath);
            this.Name = "EasyPatch";
            this.Text = "Easy Patch";
            this.Load += new System.EventHandler(this.EasyPatch_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtIsoPath;
        private System.Windows.Forms.Label lblIsoPath;
        private System.Windows.Forms.Button btnIsoPath;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.ProgressBar PatchProgressBar;
        private System.Windows.Forms.Button btnPatchPath;
        private System.Windows.Forms.Label lblPatchPath;
        private System.Windows.Forms.TextBox txtPatchPath;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.Button btnOutputPath;
        private System.Windows.Forms.Label lblOutputFile;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.OpenFileDialog dlgIsoPath;
        private System.Windows.Forms.Button btnPatch;
        private System.Windows.Forms.OpenFileDialog dlgPatchPath;
        private System.Windows.Forms.SaveFileDialog dlgOutputPath;
        private double filesProcessed;
        private double totalFiles;
        private Task patchTask;
        private string logFilePath;
        private System.Windows.Forms.RichTextBox txtPatchOutputProcess;
    }
}