using OpenQA.Selenium.Chrome;
using PlaylistSpotify.Models;
using PlaylistSpotify.Services;
using PlaylistSpotify.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PlaylistSpotify
{
    public partial class FormMain : Form
    {
        private PlaylistService playlistService { get; set; }
        public ChromeDriver chromeDriver { get; set; }
        public FormMain()
        {
            InitializeComponent();
        }

        public void FormMain_Load(object sender, EventArgs e)
        {
            List<Playlist> playlists = new List<Playlist>();
            playlistService = new PlaylistService();

            playlists = playlistService.GetPlaylistDataToUpdate(null);

            for (int i = 0; i < playlists.Count; i++)
            {
                chromeDriver = new ChromeDriver(BrowserSettings.ChromeDriverService, BrowserSettings.ChromeOptions(playlists[i].PathFolder));
                playlists[i] = playlistService.GetUpdatedPlaylist(chromeDriver, playlists[i]);
                playlistService.UpdatePlaylist(chromeDriver, playlists[i]);
                chromeDriver.Quit();
            }
        }
    }
}