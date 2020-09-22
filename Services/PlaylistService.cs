﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PlaylistSpotify.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace PlaylistSpotify.Services
{
    public class PlaylistService
    {
        List<Playlist> playlistsUpdated { get; set; }
        private IWebElement WebElement { get; set; }

        public List<Playlist> GetPlaylistDataToUpdate(string? defaultPath)
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
                WebElement = chromeDriver.FindElement(By.ClassName("TrackListHeader__entity-name"), 10);
                string[] trackSplit = WebElement.Text.Split(new[] { "\r\n" }, StringSplitOptions.None);
                playlist.Name = trackSplit[0];
            }

            var root = chromeDriver.FindElement(By.XPath("//*[@id='main']/div/div[2]/div[4]/div[1]/div/div[2]/div/div/div[2]/section/div[4]/div/div[2]/div[2]"), 1);
            var children = root.FindElement(By.XPath(".//div[1]/div/div/div[2]/div/div/span/span"));

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
                    catch (Exception e)
                    {
                        artistSongOnSpotify = root.FindElement(By.XPath("//div[" + i + "]/div/div/div[2]/div/span/a/span/span"));
                    }
                    Music music = new Music { Artist = artistSongOnSpotify.Text, Name = nameSongOnSpotify.Text };
                    playlist.Music.Add(music);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            return playlist;
        }

        public void UpdatePlaylist(ChromeDriver chromeDriver, Playlist playlist)
        {
            //foreach (var music in playlist.Music)
            //{
            int count = 1;
            for (int i = 0; i < 4; i++)
            {
                var music = playlist.Music[i];
                var track = RemoveInvalidPathChars(count + " " + music.Artist + " - " + music.Name + ".mp3");

                //get file by number
                DirectoryInfo pathDir = new DirectoryInfo(playlist.PathFolder);
                var Tracks = pathDir.GetFiles("" + count + "*.mp3");

                if (Tracks.Any())
                {

                    if (Tracks[0].Name != track)
                    {
                        File.Delete(playlist.PathFolder + "\\" + Tracks[0].Name);
                        //downloadSong
                        Download(chromeDriver, music, count);
                        //count++;
                    }
                }


                //var Tracks = Directory.GetFiles(playlist.PathFolder, "*" + track + "*").
                //  Where(s => s.EndsWith(".mp3")).
                //  Select(Path.GetFileName).ToList();

                else
                {
                    Download(chromeDriver, music, count);
                    //count++;
                    //string name = RemoveNonAlpha(music.Name).Replace(" ", "+");
                    //string artist = RemoveNonAlpha(music.Artist).Replace(" ", "+");

                    //string youtubeUrl = "https://www.youtube.com/results?search_query=" + artist + "+" + name + "+audio";

                    //chromeDriver.Navigate().GoToUrl(youtubeUrl);

                    //IReadOnlyCollection<IWebElement> webElements = chromeDriver.FindElements(By.Id("video-title"), 10);
                    ////video-title
                    //foreach (var we in webElements)
                    //{
                    //    youtubeUrl = we.GetAttribute("href");
                    //    if (youtubeUrl != null)
                    //    {
                    //        string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                    //        Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    //        music.NameAfterDownload = RemoveInvalidPathChars(we.Text);


                    //        //  Tracks = Directory.GetFiles(playlist.PathFolder, "*" + music.NameAfterDownload + "*").
                    //        //Where(s => s.EndsWith(".mp3")).
                    //        //Select(Path.GetFileName).ToList();

                    //        //  if (!Tracks.Any())
                    //        //  {
                    //        if (CheckDuration(chromeDriver))
                    //        {

                    //            DownloadFromYtmp3(chromeDriver, youtubeUrl);

                    //            music.Number = count;
                    //            count++;

                    //            //change the name of file

                    //            //var pathFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                    //            //var Tracks = Directory.GetFiles(pathFolder, "*" + track + "*").
                    //            //           Where(s => s.EndsWith(".mp3") || s.EndsWith(".part")).
                    //            //           Select(System.IO.Path.GetFileName).ToList();

                    //            //    var Tracks = Directory.GetFiles(playlist.PathFolder, "*" + we.Text + "*").
                    //            //               Where(s => s.EndsWith(".mp3") || s.EndsWith(".crdownload")).
                    //            //               Select(Path.GetFileName).ToList();

                    //            //}
                    //            break;
                    //        }
                    //        //}
                    //        break;
                    //    }
                    //}
                }
                count++;
            }

            CheckIfDownloadedAll(playlist.PathFolder);
            RenameFiles(playlist);

        }

        public void Download(ChromeDriver chromeDriver, Music music, int count)
        {
            string name = RemoveNonAlpha(music.Name).Replace(" ", "+");
            string artist = RemoveNonAlpha(music.Artist).Replace(" ", "+");

            string youtubeUrl = "https://www.youtube.com/results?search_query=" + artist + "+" + name + "+audio";

            chromeDriver.Navigate().GoToUrl(youtubeUrl);

            IReadOnlyCollection<IWebElement> webElements = chromeDriver.FindElements(By.Id("video-title"), 10);
            foreach (var we in webElements)
            {
                youtubeUrl = we.GetAttribute("href");
                if (youtubeUrl != null)
                {
                    string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    music.NameAfterDownload = RemoveInvalidPathChars(we.Text);


                    //  Tracks = Directory.GetFiles(playlist.PathFolder, "*" + music.NameAfterDownload + "*").
                    //Where(s => s.EndsWith(".mp3")).
                    //Select(Path.GetFileName).ToList();

                    //  if (!Tracks.Any())
                    //  {
                    if (CheckDuration(chromeDriver))
                    {

                        DownloadFromYtmp3(chromeDriver, youtubeUrl);

                        music.Number = count;

                        //change the name of file

                        //var pathFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                        //var Tracks = Directory.GetFiles(pathFolder, "*" + track + "*").
                        //           Where(s => s.EndsWith(".mp3") || s.EndsWith(".part")).
                        //           Select(System.IO.Path.GetFileName).ToList();

                        //    var Tracks = Directory.GetFiles(playlist.PathFolder, "*" + we.Text + "*").
                        //               Where(s => s.EndsWith(".mp3") || s.EndsWith(".crdownload")).
                        //               Select(Path.GetFileName).ToList();

                        //}
                        return;
                    }
                    //}
                    return;
                }
            }
        }

        public void RenameFiles(Playlist playlist)
        {
            foreach (var music in playlist.Music)
            {
                if (music.NameAfterDownload != null)
                {
                    var Tracks = Directory.GetFiles(playlist.PathFolder, "*" + RemoveDoubleSpaces(music.NameAfterDownload) + "*").
                          Where(s => s.EndsWith(".mp3")).
                          Select(Path.GetFileName).ToList();
                    if (Tracks.Any())
                    {
                        string newTrackName = music.Artist + " - " + music.Name + ".mp3";
                        newTrackName = music.Number + " " + RemoveInvalidPathChars(newTrackName);

                        File.Move(playlist.PathFolder + "\\" + Tracks[0], playlist.PathFolder + "\\" + newTrackName);
                    }
                }
            }
        }

        public string RemoveInvalidPathChars(string name)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(name, "");
        }

        public string RemoveDoubleSpaces(string name)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(name, " ");
        }

        public void CheckIfDownloadedAll(string pathFolder)
        {
            var tracks = Directory.GetFiles(pathFolder, "*.crdownload")
                                                            .Select(Path.GetFileName)
                                                            .ToList();
            if (tracks.Any())
            {
                Thread.Sleep(2000);
                CheckIfDownloadedAll(pathFolder);
            }
        }

        public void DownloadFromYtmp3(ChromeDriver chromeDriver, string youtubeUrl)
        {
            try
            {
                chromeDriver.Navigate().GoToUrl("https://ytmp3.cc/en13/");

                WebElement = chromeDriver.FindElement(By.Id("input"), 1);
                WebElement.SendKeys(youtubeUrl);
                WebElement = chromeDriver.FindElement(By.Id("submit"), 1);
                WebElement.Click();
                //IWebElement error = chromeDriver.FindElement(By.Id("error"),1);
                //if (error != null)
                //{

                //}
                WebElement = chromeDriver.FindElement(By.XPath("//a[contains(text(),'Download')]"), 1);
                WebElement.Click();

                List<String> tabs = new List<String>(chromeDriver.WindowHandles);

                if (tabs.Count() > 1)
                {
                    for (int i = 1; i < tabs.Count(); i++)
                    {
                        chromeDriver.SwitchTo().Window(tabs[i]);
                        chromeDriver.Navigate().GoToUrl("https://google.com");
                        chromeDriver.Close();
                    }
                }
                chromeDriver.SwitchTo().Window(tabs[0]);


            }
            catch (Exception ex)
            {
                DownloadFromYtmp3(chromeDriver, youtubeUrl);
            }
        }

        public Music FormatMusic(Music music)
        {
            if (music.Artist.Contains('•'))
            {
                string[] artist = music.Artist.Split('\n');
                if (artist[0].Contains(','))
                {
                    artist = artist[0].Split(',');
                    music.Artist = artist[0];
                }
                else if (!artist[0].Contains("E\r"))
                {
                    string[] artistTemp = artist[0].Split('\r');
                    music.Artist = artistTemp[0];
                }
                else if (artist[1].Contains(','))
                {
                    artist = artist[1].Split(',');
                    music.Artist = artist[0];
                }
                else
                {
                    if (artist[0].Contains("E\r"))
                    {
                        string[] artistTemp = artist[1].Split('\r');
                        music.Artist = artistTemp[0];
                    }
                    else
                    {
                        string[] text1 = artist[0].Split('\r');
                        music.Artist = text1[0];
                    }
                }
            }
            else
            {
                music.Artist = "";
            }
            return music;
        }
        public static string RemoveNonAlpha(string str)
        {
            string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
            string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

            for (int i = 0; i < comAcentos.Length; i++)
            {
                str = str.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
            }

            str = Regex.Replace(str, "[^a-zA-Z0-9 ]", "");
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            str = regex.Replace(str, " ");

            return str;
        }
        public bool CheckDuration(ChromeDriver chromeDriver)
        {
            bool result = false;
            try
            {
                //check if music duration is less than 9 minutes 

                var element = chromeDriver.FindElement(By.XPath("//ytd-video-renderer//ytd-thumbnail-overlay-time-status-renderer"), 50);

                string[] time = element.Text.Split(new[] { ":" }, StringSplitOptions.None);

                result = time.Length <= 2 ? true : false;
                if (result)
                {
                    result = int.Parse(time[0]) < 9 ? true : false;
                }

            }
            catch
            {
                CheckDuration(chromeDriver);
            }
            return result;
        }
    }
}