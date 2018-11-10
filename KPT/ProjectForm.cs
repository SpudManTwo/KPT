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
    public partial class ProjectForm : Form
    {
        public ProjectForm()
        {
            InitializeComponent();
            this.Text = "Working Project";
        }

        public bool CheckFolder(string fileName)
        {
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ProjectFolder.RebuildCPKs())
            {
                MessageBox.Show("CPKs rebuilt!");
            }
            else
            {
                MessageBox.Show("There was an error while rebuilding the CPKs.");
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ProjectFolder.DumpStrings())
            {
                MessageBox.Show("Strings dumped!");
            }
            else
            {
                MessageBox.Show("There was an error while dumping strings.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ProjectFolder.LoadStrings())
            {
                MessageBox.Show("Strings loaded!");
            }
            else
            {
                MessageBox.Show("There was an error while loading strings.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "ISO|*.iso";
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName == "")
            {
                return;
            }

            if (ProjectFolder.DumpISO(openFileDialog1.FileName))
            {
                MessageBox.Show("ISO dumped!");
            }
            else
            {
                MessageBox.Show("There was an error while dumping the ISO.");
            }


        }

        private void button5_Click(object sender, EventArgs e)
        {
            var dumper = new Dumper();

            if (dumper.ProcessDirectory(ProjectFolder.GetRootDir()))
            {
                MessageBox.Show("Files unpacked!");
            }
            else
            {
                MessageBox.Show("There was an error while unpacking the files.");
            }
        }
    }


}
