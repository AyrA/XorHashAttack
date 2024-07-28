namespace XorHashAttack.UI
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            TbHash = new TextBox();
            label2 = new Label();
            TbHashList = new TextBox();
            label3 = new Label();
            BtnBreakXor = new Button();
            CbOptimize = new CheckBox();
            BtnCancel = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(11, 15);
            label1.Name = "label1";
            label1.Size = new Size(64, 13);
            label1.TabIndex = 0;
            label1.Text = "Hash to find";
            // 
            // TbHash
            // 
            TbHash.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TbHash.CharacterCasing = CharacterCasing.Upper;
            TbHash.Location = new Point(92, 12);
            TbHash.Name = "TbHash";
            TbHash.Size = new Size(520, 20);
            TbHash.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(11, 41);
            label2.Name = "label2";
            label2.Size = new Size(47, 13);
            label2.TabIndex = 2;
            label2.Text = "Hash list";
            // 
            // TbHashList
            // 
            TbHashList.AllowDrop = true;
            TbHashList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TbHashList.CharacterCasing = CharacterCasing.Upper;
            TbHashList.Location = new Point(92, 38);
            TbHashList.Multiline = true;
            TbHashList.Name = "TbHashList";
            TbHashList.ScrollBars = ScrollBars.Both;
            TbHashList.Size = new Size(520, 222);
            TbHashList.TabIndex = 3;
            TbHashList.WordWrap = false;
            TbHashList.DragDrop += TbHashList_DragDrop;
            TbHashList.DragOver += ThHashList_DragOver;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(92, 271);
            label3.Name = "label3";
            label3.Size = new Size(271, 13);
            label3.TabIndex = 4;
            label3.Text = "You can drag a file onto the text box to read its contents";
            // 
            // BtnBreakXor
            // 
            BtnBreakXor.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            BtnBreakXor.Location = new Point(92, 302);
            BtnBreakXor.Name = "BtnBreakXor";
            BtnBreakXor.Size = new Size(114, 23);
            BtnBreakXor.TabIndex = 5;
            BtnBreakXor.Text = "Break XOR sum";
            BtnBreakXor.UseVisualStyleBackColor = true;
            BtnBreakXor.Click += BtnBreakXor_Click;
            // 
            // CbOptimize
            // 
            CbOptimize.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CbOptimize.AutoSize = true;
            CbOptimize.Checked = true;
            CbOptimize.CheckState = CheckState.Checked;
            CbOptimize.Location = new Point(212, 306);
            CbOptimize.Name = "CbOptimize";
            CbOptimize.Size = new Size(99, 17);
            CbOptimize.TabIndex = 6;
            CbOptimize.Text = "Optimize output";
            CbOptimize.UseVisualStyleBackColor = true;
            // 
            // BtnCancel
            // 
            BtnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnCancel.Enabled = false;
            BtnCancel.Location = new Point(541, 302);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(71, 23);
            BtnCancel.TabIndex = 7;
            BtnCancel.Text = "Cancel";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(624, 361);
            Controls.Add(CbOptimize);
            Controls.Add(BtnCancel);
            Controls.Add(BtnBreakXor);
            Controls.Add(TbHashList);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(TbHash);
            Controls.Add(label1);
            MinimumSize = new Size(460, 240);
            Name = "FrmMain";
            Text = "XOR Hash Sum Attack Generator";
            FormClosing += FrmMain_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox TbHash;
        private Label label2;
        private TextBox TbHashList;
        private Label label3;
        private Button BtnBreakXor;
        private CheckBox CbOptimize;
        private Button BtnCancel;
    }
}
