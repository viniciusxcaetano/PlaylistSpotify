using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using PlaylistSpotify.Forms;
using PlaylistSpotify.Models;
using PlaylistSpotify.Services;
using PlaylistSpotify.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlaylistSpotify
{
    public partial class FormMain : Form
    {
        private PlaylistService playlistService { get; set; }
        public ChromeDriver chromeDriver { get; set; }
        List<Playlist> playlists { get; set; }
        public static string defaultPath { get; set; }
        public bool updateForm { get; set; }
        public FormMain()
        {
            this.Activated += new EventHandler(FormMain_Activated);
            InitializeComponent();
        }

        public void FormMain_Load(object sender, EventArgs e)
        {
        }

        private void btnSearchPath_Click(object sender, EventArgs e)
        {
            defaultPath = "";

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                checkedListBoxPlaylists.Items.Clear();
                defaultPath = folderBrowserDialog1.SelectedPath;
                btnAddPlaylist.Enabled = true;
            }

            if (!string.IsNullOrEmpty(defaultPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(defaultPath);
                playlists = new List<Playlist>();

                if (directoryInfo.Exists)
                {
                    DirectoryInfo[] directorySpotify = directoryInfo.GetDirectories("*" + "Spotify" + "*.*");

                    if (directorySpotify.Any())
                    {
                        foreach (var dirSpot in directorySpotify)
                        {
                            string PathFolder = defaultPath + "\\" + dirSpot.Name;
                            string PathUrlFile = PathFolder + "\\url.txt";

                            if (File.Exists(PathUrlFile))
                            {
                                string[] pathUrlFile = File.ReadAllLines(PathUrlFile);

                                if (pathUrlFile.Any())
                                {
                                    string Url = pathUrlFile.FirstOrDefault();
                                    Playlist playlist = new Playlist(defaultPath, Url)
                                    {
                                        Name = dirSpot.Name,
                                        PathFolder = PathFolder,
                                        PathUrlFile = PathUrlFile
                                    };
                                    playlists.Add(playlist);
                                }
                                else
                                {
                                    MessageBox.Show("Url file is Empty");
                                }
                            }
                            else MessageBox.Show("Url file, doesn't exists");
                        }
                    }
                }

                checkedListBoxPlaylists.Items.AddRange(playlists.Select(p => p.Name).ToArray());
                if (checkedListBoxPlaylists.Items.Count > 0)
                {
                    btnSelectAll.Enabled = true;
                }
            }
        }
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            if (btnSelectAll.Text == "Selecionar Todas")
            {
                btnUpdate.Enabled = true;
                btnSelectAll.Text = "Limpar Seleção";
                SelectDeselectAll(true);
            }
            else
            {
                btnUpdate.Enabled = false;
                btnSelectAll.Text = "Selecionar Todas";
                SelectDeselectAll(false);
            }
        }
        void SelectDeselectAll(bool Selected)
        {
            for (int i = 0; i < checkedListBoxPlaylists.Items.Count; i++)
            {
                checkedListBoxPlaylists.SetItemChecked(i, Selected);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            playlistService = new PlaylistService();
            var selectedPlaylists = playlists.Where(p => checkedListBoxPlaylists.CheckedItems.Contains(p.Name)).ToList();

            for (int i = 0; i < selectedPlaylists.Count; i++)
            {
                chromeDriver = new ChromeDriver(BrowserSettings.ChromeDriverService, BrowserSettings.ChromeOptions(selectedPlaylists[i].PathFolder));
                selectedPlaylists[i] = playlistService.GetUpdatedPlaylist(chromeDriver, selectedPlaylists[i]);
                playlistService.UpdatePlaylist(chromeDriver, selectedPlaylists[i]);
                chromeDriver.Quit();
            }
            MessageBox.Show("Playlists atualizadas com sucesso.");
        }

        private void checkedListBoxPlaylists_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnUpdate.Enabled = checkedListBoxPlaylists.CheckedItems.Count > 0;
        }

        private void btnAddPlaylist_Click(object sender, EventArgs e)
        {
            updateForm = true;
            FormAddPlaylist formAddPlaylist = new FormAddPlaylist();
            formAddPlaylist.Show();
        }

        private void UpdateCheckedListBox()
        {
            if (!string.IsNullOrEmpty(defaultPath))
            {
                checkedListBoxPlaylists.Items.Clear();
                DirectoryInfo directoryInfo = new DirectoryInfo(defaultPath);
                playlists = new List<Playlist>();

                if (directoryInfo.Exists)
                {
                    DirectoryInfo[] directorySpotify = directoryInfo.GetDirectories("*" + "Spotify" + "*.*");

                    if (directorySpotify.Any())
                    {
                        foreach (var dirSpot in directorySpotify)
                        {
                            string PathFolder = defaultPath + "\\" + dirSpot.Name;
                            string PathUrlFile = PathFolder + "\\url.txt";

                            if (File.Exists(PathUrlFile))
                            {
                                string[] pathUrlFile = File.ReadAllLines(PathUrlFile);

                                if (pathUrlFile.Any())
                                {
                                    string Url = pathUrlFile.FirstOrDefault();
                                    Playlist playlist = new Playlist(defaultPath, Url)
                                    {
                                        Name = dirSpot.Name,
                                        PathFolder = PathFolder,
                                        PathUrlFile = PathUrlFile
                                    };
                                    playlists.Add(playlist);
                                }
                                else
                                {
                                    MessageBox.Show("Arquivo url.txt está vazio");
                                }
                            }
                            else MessageBox.Show("Arquivo url.txt não existe");
                        }
                    }
                }
                checkedListBoxPlaylists.Items.AddRange(playlists.Select(p => p.Name).ToArray());
                if (checkedListBoxPlaylists.Items.Count > 0)
                {
                    btnSelectAll.Enabled = true;
                }
            }
        }

        void FormMain_Activated(object sender, EventArgs e)
        {
            var lastOpenedForm1 = Application.OpenForms.Cast<Form>().Last();
            // or (without Linq):
            //var lastOpenedForm2 = Application.OpenForms[Application.OpenForms.Count - 1];

            if (lastOpenedForm1.Name == "FormAddPlaylist" && updateForm)
            {
                UpdateCheckedListBox();
                updateForm = false;
            }
        }
    }
}