namespace RabbitHoleMado
{
    partial class ConfigWizard
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
            this.SettingsTab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnListenNext = new System.Windows.Forms.Button();
            this.listListenIP = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnSendNext = new System.Windows.Forms.Button();
            this.btnRemoveSelectedSendIP = new System.Windows.Forms.Button();
            this.btnAddSendIP = new System.Windows.Forms.Button();
            this.inputSendIp = new System.Windows.Forms.TextBox();
            this.listSendIP = new System.Windows.Forms.ListView();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnStartListen = new System.Windows.Forms.Button();
            this.inputEncryptPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SettingsTab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // SettingsTab
            // 
            this.SettingsTab.Controls.Add(this.tabPage1);
            this.SettingsTab.Controls.Add(this.tabPage2);
            this.SettingsTab.Controls.Add(this.tabPage3);
            this.SettingsTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SettingsTab.Location = new System.Drawing.Point(0, 0);
            this.SettingsTab.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.SettingsTab.Name = "SettingsTab";
            this.SettingsTab.SelectedIndex = 0;
            this.SettingsTab.Size = new System.Drawing.Size(746, 488);
            this.SettingsTab.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnListenNext);
            this.tabPage1.Controls.Add(this.listListenIP);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(8, 45);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Size = new System.Drawing.Size(730, 435);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Listen";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnListenNext
            // 
            this.btnListenNext.Enabled = false;
            this.btnListenNext.Location = new System.Drawing.Point(608, 354);
            this.btnListenNext.Name = "btnListenNext";
            this.btnListenNext.Size = new System.Drawing.Size(91, 39);
            this.btnListenNext.TabIndex = 2;
            this.btnListenNext.Text = "Next";
            this.btnListenNext.UseVisualStyleBackColor = true;
            this.btnListenNext.Click += new System.EventHandler(this.BtnListenNext_Click);
            // 
            // listListenIP
            // 
            this.listListenIP.CheckOnClick = true;
            this.listListenIP.FormattingEnabled = true;
            this.listListenIP.Location = new System.Drawing.Point(9, 38);
            this.listListenIP.Name = "listListenIP";
            this.listListenIP.Size = new System.Drawing.Size(690, 310);
            this.listListenIP.TabIndex = 1;
            this.listListenIP.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ListListenIP_ItemCheck);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(211, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "Address to listen:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnSendNext);
            this.tabPage2.Controls.Add(this.btnRemoveSelectedSendIP);
            this.tabPage2.Controls.Add(this.btnAddSendIP);
            this.tabPage2.Controls.Add(this.inputSendIp);
            this.tabPage2.Controls.Add(this.listSendIP);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Location = new System.Drawing.Point(8, 45);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage2.Size = new System.Drawing.Size(730, 435);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Send";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnSendNext
            // 
            this.btnSendNext.Location = new System.Drawing.Point(603, 385);
            this.btnSendNext.Name = "btnSendNext";
            this.btnSendNext.Size = new System.Drawing.Size(94, 39);
            this.btnSendNext.TabIndex = 5;
            this.btnSendNext.Text = "Next";
            this.btnSendNext.UseVisualStyleBackColor = true;
            this.btnSendNext.Click += new System.EventHandler(this.BtnSendNext_Click);
            // 
            // btnRemoveSelectedSendIP
            // 
            this.btnRemoveSelectedSendIP.Enabled = false;
            this.btnRemoveSelectedSendIP.Location = new System.Drawing.Point(353, 385);
            this.btnRemoveSelectedSendIP.Name = "btnRemoveSelectedSendIP";
            this.btnRemoveSelectedSendIP.Size = new System.Drawing.Size(153, 39);
            this.btnRemoveSelectedSendIP.TabIndex = 4;
            this.btnRemoveSelectedSendIP.Text = "Remove selected";
            this.btnRemoveSelectedSendIP.UseVisualStyleBackColor = true;
            // 
            // btnAddSendIP
            // 
            this.btnAddSendIP.Location = new System.Drawing.Point(231, 386);
            this.btnAddSendIP.Name = "btnAddSendIP";
            this.btnAddSendIP.Size = new System.Drawing.Size(102, 39);
            this.btnAddSendIP.TabIndex = 3;
            this.btnAddSendIP.Text = "Add new";
            this.btnAddSendIP.UseVisualStyleBackColor = true;
            this.btnAddSendIP.Click += new System.EventHandler(this.BtnAddSendIP_Click);
            // 
            // inputSendIp
            // 
            this.inputSendIp.Location = new System.Drawing.Point(4, 386);
            this.inputSendIp.Name = "inputSendIp";
            this.inputSendIp.Size = new System.Drawing.Size(217, 39);
            this.inputSendIp.TabIndex = 2;
            // 
            // listSendIP
            // 
            this.listSendIP.HideSelection = false;
            this.listSendIP.Location = new System.Drawing.Point(8, 38);
            this.listSendIP.Name = "listSendIP";
            this.listSendIP.Size = new System.Drawing.Size(689, 342);
            this.listSendIP.TabIndex = 1;
            this.listSendIP.UseCompatibleStateImageBehavior = false;
            this.listSendIP.View = System.Windows.Forms.View.List;
            this.listSendIP.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListSendIP_MouseUp);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(218, 31);
            this.label2.TabIndex = 0;
            this.label2.Text = "IP to send packet:";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnStartListen);
            this.tabPage3.Controls.Add(this.inputEncryptPassword);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Location = new System.Drawing.Point(8, 45);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(730, 435);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Other";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnStartListen
            // 
            this.btnStartListen.Location = new System.Drawing.Point(597, 371);
            this.btnStartListen.Name = "btnStartListen";
            this.btnStartListen.Size = new System.Drawing.Size(99, 41);
            this.btnStartListen.TabIndex = 2;
            this.btnStartListen.Text = "Start";
            this.btnStartListen.UseVisualStyleBackColor = true;
            this.btnStartListen.Click += new System.EventHandler(this.BtnStartListen_Click);
            // 
            // inputEncryptPassword
            // 
            this.inputEncryptPassword.Location = new System.Drawing.Point(12, 46);
            this.inputEncryptPassword.Name = "inputEncryptPassword";
            this.inputEncryptPassword.Size = new System.Drawing.Size(685, 39);
            this.inputEncryptPassword.TabIndex = 1;
            this.inputEncryptPassword.Text = "bilibilibiniconiconi";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(269, 31);
            this.label3.TabIndex = 0;
            this.label3.Text = "En(De)crypt password:";
            // 
            // ConfigWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 488);
            this.Controls.Add(this.SettingsTab);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ConfigWizard";
            this.Text = "ConfigWizard";
            this.Load += new System.EventHandler(this.ConfigWizard_Load);
            this.SettingsTab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl SettingsTab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnListenNext;
        private System.Windows.Forms.CheckedListBox listListenIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnRemoveSelectedSendIP;
        private System.Windows.Forms.Button btnAddSendIP;
        private System.Windows.Forms.TextBox inputSendIp;
        private System.Windows.Forms.ListView listSendIP;
        private System.Windows.Forms.Button btnStartListen;
        private System.Windows.Forms.TextBox inputEncryptPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSendNext;
    }
}