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
            this.radioButtonSplit = new System.Windows.Forms.RadioButton();
            this.radioButtonMerge = new System.Windows.Forms.RadioButton();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.labelAction = new System.Windows.Forms.Label();
            this.lblFile = new System.Windows.Forms.Label();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.lblType = new System.Windows.Forms.Label();
            this.cmbMethod = new System.Windows.Forms.ComboBox();
            this.cmbQuantityOfParts = new System.Windows.Forms.ComboBox();
            this.lblPartNumber = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // radioButtonSplit
            // 
            this.radioButtonSplit.AutoSize = true;
            this.radioButtonSplit.Location = new System.Drawing.Point(87, 15);
            this.radioButtonSplit.Name = "radioButtonSplit";
            this.radioButtonSplit.Size = new System.Drawing.Size(45, 17);
            this.radioButtonSplit.TabIndex = 0;
            this.radioButtonSplit.TabStop = true;
            this.radioButtonSplit.Text = "&Split";
            this.radioButtonSplit.UseVisualStyleBackColor = true;
            // 
            // radioButtonMerge
            // 
            this.radioButtonMerge.AutoSize = true;
            this.radioButtonMerge.Location = new System.Drawing.Point(155, 15);
            this.radioButtonMerge.Name = "radioButtonMerge";
            this.radioButtonMerge.Size = new System.Drawing.Size(55, 17);
            this.radioButtonMerge.TabIndex = 1;
            this.radioButtonMerge.TabStop = true;
            this.radioButtonMerge.Text = "&Merge";
            this.radioButtonMerge.UseVisualStyleBackColor = true;
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(87, 44);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(342, 20);
            this.txtFilePath.TabIndex = 2;
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // labelAction
            // 
            this.labelAction.AutoSize = true;
            this.labelAction.Location = new System.Drawing.Point(17, 17);
            this.labelAction.Name = "labelAction";
            this.labelAction.Size = new System.Drawing.Size(37, 13);
            this.labelAction.TabIndex = 0;
            this.labelAction.Text = "&Action";
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(17, 51);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(23, 13);
            this.lblFile.TabIndex = 2;
            this.lblFile.Text = "&File";
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(435, 42);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(24, 23);
            this.btnSelectFile.TabIndex = 3;
            this.btnSelectFile.Text = "...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(17, 85);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(43, 13);
            this.lblType.TabIndex = 4;
            this.lblType.Text = "M&ethod";
            // 
            // cmbMethod
            // 
            this.cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMethod.FormattingEnabled = true;
            this.cmbMethod.Items.AddRange(new object[] {
            "Confettis",
            "Shamirs"});
            this.cmbMethod.Location = new System.Drawing.Point(87, 82);
            this.cmbMethod.Name = "cmbMethod";
            this.cmbMethod.Size = new System.Drawing.Size(121, 21);
            this.cmbMethod.TabIndex = 5;
            // 
            // cmbQuantityOfParts
            // 
            this.cmbQuantityOfParts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbQuantityOfParts.FormattingEnabled = true;
            this.cmbQuantityOfParts.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbQuantityOfParts.Location = new System.Drawing.Point(338, 82);
            this.cmbQuantityOfParts.Name = "cmbQuantityOfParts";
            this.cmbQuantityOfParts.Size = new System.Drawing.Size(121, 21);
            this.cmbQuantityOfParts.TabIndex = 6;
            // 
            // lblPartNumber
            // 
            this.lblPartNumber.AutoSize = true;
            this.lblPartNumber.Location = new System.Drawing.Point(237, 85);
            this.lblPartNumber.Name = "lblPartNumber";
            this.lblPartNumber.Size = new System.Drawing.Size(84, 13);
            this.lblPartNumber.TabIndex = 7;
            this.lblPartNumber.Text = "Quantity of parts";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(155, 136);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(177, 26);
            this.btnStart.TabIndex = 8;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 180);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblPartNumber);
            this.Controls.Add(this.cmbQuantityOfParts);
            this.Controls.Add(this.cmbMethod);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.labelAction);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.radioButtonMerge);
            this.Controls.Add(this.radioButtonSplit);
            this.Name = "MainForm";
            this.Text = "FileSplitter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonSplit;
        private System.Windows.Forms.RadioButton radioButtonMerge;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label labelAction;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cmbMethod;
        private System.Windows.Forms.ComboBox cmbQuantityOfParts;
        private System.Windows.Forms.Label lblPartNumber;
        private System.Windows.Forms.Button btnStart;
    }
}

