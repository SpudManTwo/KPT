namespace KPT
{
    partial class About
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("S-KPT - SpudManTwo");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("KPT - dl471");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Disc Utils");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("SharpYAML - xoofx");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("CsvHelper - Josh Close");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Nuget Packages", new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4,
            treeNode5});
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("LibCPK/CPKTools - wmltogether");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Pgftools - tpunix");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Other Code Repos", new System.Windows.Forms.TreeNode[] {
            treeNode7,
            treeNode8});
            System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("GimConv  - Sony");
            System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("UMD-ReplaceK - SpudManTwo, Dormanil");
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Tools:", new System.Windows.Forms.TreeNode[] {
            treeNode10,
            treeNode11});
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "VersionInfoHere";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.treeView1);
            this.groupBox1.Location = new System.Drawing.Point(15, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(301, 227);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Acknowledgements";
            // 
            // treeView1
            // 
            this.treeView1.CausesValidation = false;
            this.treeView1.Location = new System.Drawing.Point(7, 20);
            this.treeView1.Name = "treeView1";
            treeNode1.Name = "S-KPT";
            treeNode1.Text = "S-KPT - SpudManTwo";
            treeNode2.Name = "KPT";
            treeNode2.Text = "KPT - dl471";
            treeNode3.Name = "Disc Utils";
            treeNode3.Text = "Disc Utils";
            treeNode4.Name = "SharpYAML - xoofx";
            treeNode4.Text = "SharpYAML - xoofx";
            treeNode5.Name = "CsvHelper";
            treeNode5.Text = "CsvHelper - Josh Close";
            treeNode6.Name = "Nuget Packages";
            treeNode6.Text = "Nuget Packages";
            treeNode7.Name = "LibCPK/CPKTools";
            treeNode7.Text = "LibCPK/CPKTools - wmltogether";
            treeNode8.Name = "Pgftools";
            treeNode8.Text = "Pgftools - tpunix";
            treeNode9.Name = "Other Code Repos";
            treeNode9.Text = "Other Code Repos";
            treeNode10.Name = "GimConv";
            treeNode10.Text = "GimConv  - Sony";
            treeNode11.Name = "UMD-ReplaceK";
            treeNode11.Text = "UMD-ReplaceK - SpudManTwo, Dormanil";
            treeNode12.Name = "Tools:";
            treeNode12.Text = "Tools:";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode6,
            treeNode9,
            treeNode12});
            this.treeView1.Size = new System.Drawing.Size(288, 201);
            this.treeView1.TabIndex = 0;
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 276);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "About";
            this.Text = "About";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TreeView treeView1;
    }
}