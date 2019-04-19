using FileSplitterDef;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSplitterWF
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "Shard files (*."+ FileSplitterCommon.FILE_EXT + ")|*." + FileSplitterCommon.FILE_EXT + "|All files (*.*)|*.*";
            if (radioButtonMerge.Checked)
                openFileDialog.FilterIndex = 1;
            else
                openFileDialog.FilterIndex = 2;

            openFileDialog.FileName = "";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog.FileName;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

        }
    }
}
