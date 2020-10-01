namespace PlaylistSpotify
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.label1 = new System.Windows.Forms.Label();
            this.btnSearchPath = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.checkedListBoxPlaylists = new System.Windows.Forms.CheckedListBox();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnAddPlaylist = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(67, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(187, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Selecione o caminho de suas músicas";
            // 
            // btnSearchPath
            // 
            this.btnSearchPath.Location = new System.Drawing.Point(272, 37);
            this.btnSearchPath.Name = "btnSearchPath";
            this.btnSearchPath.Size = new System.Drawing.Size(75, 23);
            this.btnSearchPath.TabIndex = 1;
            this.btnSearchPath.Text = "Procurar";
            this.btnSearchPath.UseVisualStyleBackColor = true;
            this.btnSearchPath.Click += new System.EventHandler(this.btnSearchPath_Click);
            // 
            // checkedListBoxPlaylists
            // 
            this.checkedListBoxPlaylists.CheckOnClick = true;
            this.checkedListBoxPlaylists.FormattingEnabled = true;
            this.checkedListBoxPlaylists.Location = new System.Drawing.Point(56, 78);
            this.checkedListBoxPlaylists.Name = "checkedListBoxPlaylists";
            this.checkedListBoxPlaylists.Size = new System.Drawing.Size(299, 319);
            this.checkedListBoxPlaylists.TabIndex = 3;
            this.checkedListBoxPlaylists.SelectedIndexChanged += new System.EventHandler(this.checkedListBoxPlaylists_SelectedIndexChanged);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Enabled = false;
            this.btnSelectAll.Location = new System.Drawing.Point(56, 403);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(108, 23);
            this.btnSelectAll.TabIndex = 4;
            this.btnSelectAll.Text = "Selecionar Todas";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Enabled = false;
            this.btnUpdate.Location = new System.Drawing.Point(272, 403);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 5;
            this.btnUpdate.Text = "Atualizar";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnAddPlaylist
            // 
            this.btnAddPlaylist.Enabled = false;
            this.btnAddPlaylist.Location = new System.Drawing.Point(170, 403);
            this.btnAddPlaylist.Name = "btnAddPlaylist";
            this.btnAddPlaylist.Size = new System.Drawing.Size(96, 23);
            this.btnAddPlaylist.TabIndex = 6;
            this.btnAddPlaylist.Text = "Adicionar Playlist";
            this.btnAddPlaylist.UseVisualStyleBackColor = true;
            this.btnAddPlaylist.Click += new System.EventHandler(this.btnAddPlaylist_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 450);
            this.Controls.Add(this.btnAddPlaylist);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this.checkedListBoxPlaylists);
            this.Controls.Add(this.btnSearchPath);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Download Playlist Spotify";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSearchPath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckedListBox checkedListBoxPlaylists;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnAddPlaylist;
    }
}

