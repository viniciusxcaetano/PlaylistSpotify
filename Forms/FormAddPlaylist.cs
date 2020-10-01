using OpenQA.Selenium.Chrome;
using PlaylistSpotify.Models;
using PlaylistSpotify.Services;
using PlaylistSpotify.Utils;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlaylistSpotify.Forms
{
    public partial class FormAddPlaylist : Form
    {
        public ChromeDriver chromeDriver { get; set; }
        private PlaylistService playlistService { get; set; }

        public FormAddPlaylist()
        {
            InitializeComponent();
        }

        private void btnAddPlaylist_Click(object sender, EventArgs e)
        {
            string path = FormMain.defaultPath;
            string url = textBoxUrl.Text;
            playlistService = new PlaylistService();
            chromeDriver = new ChromeDriver(BrowserSettings.ChromeDriverService, BrowserSettings.ChromeOptions(null));
            Playlist playlist = new Playlist(path, url);

            playlist = playlistService.GetUpdatedPlaylist(chromeDriver, playlist);
            string result = "";

            string[] splited = playlist.PathFolder.Split(new[] { "\\" }, StringSplitOptions.None);
            splited[splited.Length - 1] = "Spotify-" + splited.LastOrDefault();
            result = string.Join("\\", splited);
            //foreach(var split in sp)

            DirectoryInfo di = Directory.CreateDirectory(result);
            using (StreamWriter sw = File.CreateText(result + "\\url.txt"))
            {
                sw.WriteLine(playlist.Url);
            }
            chromeDriver.Quit();
            this.Close();
        }

        private void textBoxUrl_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxUrl.Text))
            {
                btnAddPlaylist.Enabled = true;
            }
            else
            {
                btnAddPlaylist.Enabled = false;
            }
        }
    }
}
