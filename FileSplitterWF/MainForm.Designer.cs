namespace FileSplitterWF
{
    partial class MainForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.RadioButtonSplit = new System.Windows.Forms.RadioButton();
            this.RadioButtonMerge = new System.Windows.Forms.RadioButton();
            this.TxtFilePath = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.LabelAction = new System.Windows.Forms.Label();
            this.LblFile = new System.Windows.Forms.Label();
            this.BtnSelectFile = new System.Windows.Forms.Button();
            this.LblType = new System.Windows.Forms.Label();
            this.CmbMethod = new System.Windows.Forms.ComboBox();
            this.CmbQuantityOfParts = new System.Windows.Forms.ComboBox();
            this.LblPartNumber = new System.Windows.Forms.Label();
            this.BtnStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // RadioButtonSplit
            // 
            this.RadioButtonSplit.AutoSize = true;
            this.RadioButtonSplit.Location = new System.Drawing.Point(87, 15);
            this.RadioButtonSplit.Name = "RadioButtonSplit";
            this.RadioButtonSplit.Size = new System.Drawing.Size(45, 17);
            this.RadioButtonSplit.TabIndex = 0;
            this.RadioButtonSplit.TabStop = true;
            this.RadioButtonSplit.Text = "&Split";
            this.RadioButtonSplit.UseVisualStyleBackColor = true;
            // 
            // RadioButtonMerge
            // 
            this.RadioButtonMerge.AutoSize = true;
            this.RadioButtonMerge.Location = new System.Drawing.Point(155, 15);
            this.RadioButtonMerge.Name = "RadioButtonMerge";
            this.RadioButtonMerge.Size = new System.Drawing.Size(55, 17);
            this.RadioButtonMerge.TabIndex = 1;
            this.RadioButtonMerge.TabStop = true;
            this.RadioButtonMerge.Text = "&Merge";
            this.RadioButtonMerge.UseVisualStyleBackColor = true;
            // 
            // TxtFilePath
            // 
            this.TxtFilePath.Location = new System.Drawing.Point(87, 44);
            this.TxtFilePath.Name = "TxtFilePath";
            this.TxtFilePath.Size = new System.Drawing.Size(342, 20);
            this.TxtFilePath.TabIndex = 2;
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // LabelAction
            // 
            this.LabelAction.AutoSize = true;
            this.LabelAction.Location = new System.Drawing.Point(17, 17);
            this.LabelAction.Name = "LabelAction";
            this.LabelAction.Size = new System.Drawing.Size(37, 13);
            this.LabelAction.TabIndex = 0;
            this.LabelAction.Text = "&Action";
            // 
            // LblFile
            // 
            this.LblFile.AutoSize = true;
            this.LblFile.Location = new System.Drawing.Point(17, 51);
            this.LblFile.Name = "LblFile";
            this.LblFile.Size = new System.Drawing.Size(23, 13);
            this.LblFile.TabIndex = 2;
            this.LblFile.Text = "&File";
            // 
            // BtnSelectFile
            // 
            this.BtnSelectFile.Location = new System.Drawing.Point(435, 42);
            this.BtnSelectFile.Name = "BtnSelectFile";
            this.BtnSelectFile.Size = new System.Drawing.Size(24, 23);
            this.BtnSelectFile.TabIndex = 3;
            this.BtnSelectFile.Text = "...";
            this.BtnSelectFile.UseVisualStyleBackColor = true;
            this.BtnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);
            // 
            // LblType
            // 
            this.LblType.AutoSize = true;
            this.LblType.Location = new System.Drawing.Point(17, 85);
            this.LblType.Name = "LblType";
            this.LblType.Size = new System.Drawing.Size(43, 13);
            this.LblType.TabIndex = 4;
            this.LblType.Text = "M&ethod";
            // 
            // CmbMethod
            // 
            this.CmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbMethod.FormattingEnabled = true;
            this.CmbMethod.Items.AddRange(new object[] {
            "Confettis",
            "Shamirs"});
            this.CmbMethod.Location = new System.Drawing.Point(87, 82);
            this.CmbMethod.Name = "CmbMethod";
            this.CmbMethod.Size = new System.Drawing.Size(121, 21);
            this.CmbMethod.TabIndex = 5;
            // 
            // CmbQuantityOfParts
            // 
            this.CmbQuantityOfParts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbQuantityOfParts.FormattingEnabled = true;
            this.CmbQuantityOfParts.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.CmbQuantityOfParts.Location = new System.Drawing.Point(338, 82);
            this.CmbQuantityOfParts.Name = "CmbQuantityOfParts";
            this.CmbQuantityOfParts.Size = new System.Drawing.Size(121, 21);
            this.CmbQuantityOfParts.TabIndex = 6;
            // 
            // LblPartNumber
            // 
            this.LblPartNumber.AutoSize = true;
            this.LblPartNumber.Location = new System.Drawing.Point(237, 85);
            this.LblPartNumber.Name = "LblPartNumber";
            this.LblPartNumber.Size = new System.Drawing.Size(84, 13);
            this.LblPartNumber.TabIndex = 7;
            this.LblPartNumber.Text = "Quantity of parts";
            // 
            // BtnStart
            // 
            this.BtnStart.Location = new System.Drawing.Point(155, 136);
            this.BtnStart.Name = "BtnStart";
            this.BtnStart.Size = new System.Drawing.Size(177, 26);
            this.BtnStart.TabIndex = 8;
            this.BtnStart.Text = "Start";
            this.BtnStart.UseVisualStyleBackColor = true;
            this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 180);
            this.Controls.Add(this.BtnStart);
            this.Controls.Add(this.LblPartNumber);
            this.Controls.Add(this.CmbQuantityOfParts);
            this.Controls.Add(this.CmbMethod);
            this.Controls.Add(this.LblType);
            this.Controls.Add(this.BtnSelectFile);
            this.Controls.Add(this.LblFile);
            this.Controls.Add(this.LabelAction);
            this.Controls.Add(this.TxtFilePath);
            this.Controls.Add(this.RadioButtonMerge);
            this.Controls.Add(this.RadioButtonSplit);
            this.Name = "MainForm";
            this.Text = "FileSplitter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton RadioButtonSplit;
        private System.Windows.Forms.RadioButton RadioButtonMerge;
        private System.Windows.Forms.TextBox TxtFilePath;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label LabelAction;
        private System.Windows.Forms.Label LblFile;
        private System.Windows.Forms.Button BtnSelectFile;
        private System.Windows.Forms.Label LblType;
        private System.Windows.Forms.ComboBox CmbMethod;
        private System.Windows.Forms.ComboBox CmbQuantityOfParts;
        private System.Windows.Forms.Label LblPartNumber;
        private System.Windows.Forms.Button BtnStart;
    }
}

