using OpenQA.Selenium;
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
                try
                {
                    IWebElement nameSongOnSpotify = root.FindElement(By.XPath("//div[" + i + "]/div/div/div[2]/div/div/span/span"));
                    IWebElement artistSongOnSpotify = root.FindElement(By.XPath("//div[" + i + "]/div/div/div[2]/div/span[2]/a[1]/span/span"));
                    Music music = new Music { Artist = artistSongOnSpotify.Text, Name = nameSongOnSpotify.Text };
                    playlist.Music.Add(music);
                }
                catch (Exception ex)
                {
                    break;
                }
            }



            //name song
            //*[@id="main"]/div/div[2]/div[4]/div[1]/div/div[2]/div/div/div[2]/section/div[4]/div/div[2]/div[2]/div[1]/div/div/div[2]/div/div/span/span
            //*[@id="main"]/div/div[2]/div[4]/div[1]/div/div[2]/div/div/div[2]/section/div[4]/div/div[2]/div[2]/div[50]/div/div/div[2]/div/div/span/span


            //artist
            //*[@id="main"]/div/div[2]/div[4]/div[1]/div/div[2]/div/div/div[2]/section/div[4]/div/div[2]/div[2]/div[1]/div/div/div[2]/div/span[2]/a[1]/span/span
            //*[@id="main"]/div/div[2]/div[4]/div[1]/div/div[2]/div/div/div[2]/section/div[4]/div/div[2]/div[2]/div[50]/div/div/div[2]/div/span[2]/a[1]/span/span

            //IReadOnlyCollection<IWebElement> artistsOnSpotify = songsOnSpotify.FindElements(By.XPath(""));

            //foreach (var sa in artistsOnSpotify.Zip(songsOnSpotify, Tuple.Create))
            //{
            //    Music music = FormatMusic(new Music { Artist = sa.Item1.Text, Name = sa.Item2.Text });

            //    playlist.Music.Add(music);
            //}

            return playlist;
        }

        public void UpdatePlaylist(ChromeDriver chromeDriver, Playlist playlist)
        {
            foreach (var music in playlist.Music)
            {
                string name = RemoveNonAlpha(music.Name).Replace(" ", "+");
                string artist = RemoveNonAlpha(music.Artist).Replace(" ", "+");

                string youtubeUrl = "https://www.youtube.com/results?search_query=" + artist + "+" + name + "+audio";

                chromeDriver.Navigate().GoToUrl(youtubeUrl);

                IReadOnlyCollection<IWebElement> webElements = chromeDriver.FindElements(By.Id("thumbnail"), 10);

                foreach (var we in webElements)
                {
                    youtubeUrl = we.GetAttribute("href");
                    if (!youtubeUrl.Contains("www.googleadservices.com"))
                    {
                        if (CheckDuration(chromeDriver))
                        {
                            TryDownload(chromeDriver, youtubeUrl);
                        }
                        break;
                    }
                }
            }
        }

        public void TryDownload(ChromeDriver chromeDriver, string youtubeUrl)
        {
            try
            {
                chromeDriver.Navigate().GoToUrl("https://ytmp3.cc/en13/");

                WebElement = chromeDriver.FindElement(By.Id("input"), 1);
                WebElement.SendKeys(youtubeUrl);
                WebElement = chromeDriver.FindElement(By.Id("submit"), 1);
                WebElement.Click();
                WebElement = chromeDriver.FindElement(By.XPath("//a[contains(text(),'Download')]"), 1);
                WebElement.Click();
            }
            catch (Exception ex)
            {
                TryDownload(chromeDriver, youtubeUrl);
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