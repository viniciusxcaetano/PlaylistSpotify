using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PlaylistSpotify.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PlaylistSpotify.Services
{
    public class PlaylistService
    {
        List<Playlist> playlistsUpdated { get; set; }
        private IWebElement WebElement { get; set; }
        public List<Playlist> GetPlaylistDataToUpdate(string defaultPath)
        {
            if (string.IsNullOrWhiteSpace(defaultPath))
            {
                defaultPath = "D:\\Music";
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(defaultPath);
            playlistsUpdated = new List<Playlist>();

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
                                playlistsUpdated.Add(playlist);
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
            return playlistsUpdated;
        }
        public Playlist GetUpdatedPlaylist(ChromeDriver chromeDriver, Playlist playlist)
        {
            playlist.PathFolder = playlist.Device + "\\" + playlist.Name;
            playlist.PathUrlFile = playlist.PathFolder + "\\url.txt";

            playlist.Music = new List<Music>();
            chromeDriver.Navigate().GoToUrl(playlist.Url);

            if (playlist.Name == "") // need this for the window when small
            {
                WebElement = chromeDriver.FindElement(By.ClassName("TrackListHeader__entity-name"), 1, 8);
                string[] trackSplit = WebElement.Text.Split(new[] { "\r\n" }, StringSplitOptions.None);
                playlist.Name = trackSplit[0];
            }

            var root = chromeDriver.FindElement(By.XPath("//*[@id='main']/div/div[2]/div[4]/div[1]/div/div[2]/div/div/div[2]/section/div[4]/div/div[2]/div[2]"), 1, 8);

            for (int i = 1; i < 1000; i++)
            {
                IWebElement nameSongOnSpotify;
                IWebElement artistSongOnSpotify;
                try
                {
                    nameSongOnSpotify = root.FindElement(By.XPath("//div[" + i + "]/div/div/div[2]/div/div/span/span"));

                    try
                    {
                        artistSongOnSpotify = root.FindElement(By.XPath("//div[" + i + "]/div/div/div[2]/div/span[2]/a[1]/span/span"));
                    }
                    catch (Exception)
                    {
                        artistSongOnSpotify = root.FindElement(By.XPath("//div[" + i + "]/div/div/div[2]/div/span/a/span/span"));
                    }
                    Music music = new Music { Artist = artistSongOnSpotify.Text, Name = nameSongOnSpotify.Text };
                    playlist.Music.Add(music);
                }
                catch (Exception)
                {
                    break;
                }
            }
            return playlist;
        }
        public void UpdatePlaylist(ChromeDriver chromeDriver, Playlist playlist)
        {
            int count = 1;

            foreach (var music in playlist.Music)
            {
                //for (int i = 0; i < 45; i++)
                //{
                //var music = playlist.Music[i];
                string track;
                if (count < 10)
                {
                    track = BaseService.RemoveInvalidPathChars("0" + count + " " + music.Artist + " - " + music.Name + ".mp3");
                }
                else
                {
                    track = BaseService.RemoveInvalidPathChars(count + " " + music.Artist + " - " + music.Name + ".mp3");
                }

                //Get file by number
                DirectoryInfo pathDir = new DirectoryInfo(playlist.PathFolder);
                FileInfo[] Tracks;

                if (count < 10)
                {
                    Tracks = pathDir.GetFiles("" + "0" + +count + "*.mp3");
                }
                else
                {
                    Tracks = pathDir.GetFiles("" + +count + "*.mp3");
                }

                if (Tracks.Any())
                {
                    if (Tracks.FirstOrDefault().Name != track)
                    {
                        File.Delete(playlist.PathFolder + "\\" + Tracks[0].Name);
                        music.NameAfterDownload = BaseService.Download(chromeDriver, playlist.PathFolder, music, count);
                    }
                }
                else
                {
                    music.NameAfterDownload = BaseService.Download(chromeDriver, playlist.PathFolder, music, count);
                }
                count++;
            }
            BaseService.WaitToDownload(playlist.PathFolder);
            BaseService.RenameFiles(playlist);
        }
    }
}