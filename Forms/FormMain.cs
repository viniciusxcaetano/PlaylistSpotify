using NUnit.Framework;
using OpenQA.Selenium.Chrome;
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
        public FormMain()
        {
            InitializeComponent();
        }

        public void FormMain_Load(object sender, EventArgs e)
        {
            //List<Playlist> playlists = new List<Playlist>();
            //playlistService = new PlaylistService();

            //playlists = playlistService.GetPlaylistDataToUpdate(null);

            //for (int i = 0; i < playlists.Count; i++)
            //{
            //    chromeDriver = new ChromeDriver(BrowserSettings.ChromeDriverService, BrowserSettings.ChromeOptions(playlists[i].PathFolder));
            //    playlists[i] = playlistService.GetUpdatedPlaylist(chromeDriver, playlists[i]);
            //    playlistService.UpdatePlaylist(chromeDriver, playlists[i]);
            //    chromeDriver.Quit();
            //}
        }

        private void btnSearchPath_Click(object sender, EventArgs e)
        {
            string defaultPath = "";

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                checkedListBoxPlaylists.Items.Clear();
                defaultPath = folderBrowserDialog1.SelectedPath;
            }

            //if (string.IsNullOrWhiteSpace(defaultPath))
            //{
            //    defaultPath = "D:\\Music";
            //}
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

                //var namePlaylist = playlists.Select(p => p.Name).ToList();
                //foreach (var name in namePlaylist)
                //{
                //    string[] splited = name.Split(new[] { "Spotify-" }, StringSplitOptions.None);
                //    checkedListBoxPlaylists.Items.Add(splited[1]);
                //}
                checkedListBoxPlaylists.Items.AddRange(playlists.Select(p => p.Name).ToArray());
                if (checkedListBoxPlaylists.Items.Count > 0)
                {
                    btnSelectAll.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Nenhuma playlist do Spotify foi localizada.");
                }
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            if (btnSelectAll.Text == "Selecionar Todas")
            {
                btnUpdate.Enabled = true;
                btnSelectAll.Text = "Limpar Seleção";
                SelectDeselectAll(true); // passing <strong>true </strong>so that all items will be checked
            }
            else
            {
                btnUpdate.Enabled = false;
                btnSelectAll.Text = "Selecionar Todas";
                SelectDeselectAll(false); // passing false so that all items will be unchecked
            }
        }
        void SelectDeselectAll(bool Selected)
        {
            for (int i = 0; i < checkedListBoxPlaylists.Items.Count; i++) // loop to set all items checked or unchecked
            {
                checkedListBoxPlaylists.SetItemChecked(i, Selected);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            playlistService = new PlaylistService();
            List<ChromeDriver> drivers = new List<ChromeDriver>();
            var selectedPlaylists = playlists.Where(p => checkedListBoxPlaylists.CheckedItems.Contains(p.Name)).ToList();

            for (int i = 0; i < selectedPlaylists.Count; i++)
            {
                chromeDriver = new ChromeDriver(BrowserSettings.ChromeDriverService, BrowserSettings.ChromeOptions(selectedPlaylists[i].PathFolder));
                drivers.Add(chromeDriver);
                selectedPlaylists[i] = playlistService.GetUpdatedPlaylist(drivers[i], selectedPlaylists[i]);
                playlistService.UpdatePlaylist(chromeDriver, selectedPlaylists[i]);
                chromeDriver.Quit();
            }
            MessageBox.Show("Playlists atualizadas com sucesso.");
        }

        private void checkedListBoxPlaylists_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnUpdate.Enabled = checkedListBoxPlaylists.CheckedItems.Count > 0;
        }
    }
}