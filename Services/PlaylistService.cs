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
                WebElement = chromeDriver.FindElement(By.ClassName("TrackListHeader__entity-name"), 1);
                string[] trackSplit = WebElement.Text.Split(new[] { "\r\n" }, StringSplitOptions.None);
                playlist.Name = trackSplit[0];
            }

            var root = chromeDriver.FindElement(By.XPath("//*[@id='main']/div/div[2]/div[4]/div[1]/div/div[2]/div/div/div[2]/section/div[4]/div/div[2]/div[2]"), 1);

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
                //for (int i = 0; i < 10; i++)
                //{
                //    var music = playlist.Music[i];
                var track = RemoveInvalidPathChars(count + " " + music.Artist + " - " + music.Name + ".mp3");

                //Get file by number
                DirectoryInfo pathDir = new DirectoryInfo(playlist.PathFolder);
                var Tracks = pathDir.GetFiles("" + count + "*.mp3");

                if (Tracks.Any())
                {
                    if (Tracks.FirstOrDefault().Name != track)
                    {
                        File.Delete(playlist.PathFolder + "\\" + Tracks[0].Name);
                        music.NameAfterDownload = Download(chromeDriver, playlist.PathFolder, music, count);
                    }
                }
                else
                {
                    music.NameAfterDownload = Download(chromeDriver, playlist.PathFolder, music, count);
                }
                count++;
            }
            CheckIfDownloadedAll(playlist.PathFolder);
            RenameFiles(playlist);
        }

        public string Download(ChromeDriver chromeDriver, string pathFolder, Music music, int count)
        {
            string name = RemoveNonAlpha(music.Name).Replace(" ", "+");
            string artist = RemoveNonAlpha(music.Artist).Replace(" ", "+");
            string downloadedTrack = "";
            string youtubeSearch = "https://www.youtube.com/results?search_query=" + artist + "+" + name + "+audio";
            chromeDriver.Navigate().GoToUrl(youtubeSearch);
            IReadOnlyCollection<IWebElement> webElements = chromeDriver.FindElements(By.Id("video-title"), 1);
            List<string> youtubeUrls = new List<string>();

            foreach (var we in webElements)
            {
                youtubeUrls.Add(we.GetAttribute("href"));
            }

            for (int i = 0; i < youtubeUrls.Count; i++)
            {
                string youtubeUrl = youtubeUrls[i];

                if (youtubeUrl != null)
                {
                    music.Number = count;
                    downloadedTrack = YtMp3(chromeDriver, pathFolder, youtubeUrl);

                    if (!string.IsNullOrEmpty(downloadedTrack))
                    {
                        break;
                    }
                }
            }
            return downloadedTrack;
        }
        public string YtMp3(ChromeDriver chromeDriver, string pathFolder, string youtubeUrl)
        {
            string downloadedTrack = "";
            try
            {
                chromeDriver.Navigate().GoToUrl("https://ytmp3.cc/en13/");
                WebElement = chromeDriver.FindElement(By.Id("input"), 1);
                WebElement.SendKeys(youtubeUrl);
                WebElement = chromeDriver.FindElement(By.Id("submit"), 1);
                WebElement.Click();
                var listTracks1 = Directory.GetFiles(pathFolder, "*").Where(s => s.EndsWith(".crdownload")).Select(Path.GetFileName).ToList();
                WebElement = chromeDriver.FindElement(By.XPath("//a[contains(text(),'Download')]"), 1);

                if (WebElement == null)
                {
                    return "";
                }

                WebElement.Click();
                List<String> tabs = new List<String>(chromeDriver.WindowHandles);

                //To close Ad.
                if (tabs.Count() > 1)
                {
                    for (int i = 1; i < tabs.Count(); i++)
                    {
                        chromeDriver.SwitchTo().Window(tabs[i]);
                        chromeDriver.Close();
                    }
                }
                chromeDriver.SwitchTo().Window(tabs[0]);

                while (String.IsNullOrEmpty(downloadedTrack))
                {
                    var listTracks2 = Directory.GetFiles(pathFolder, "*").Where(s => s.EndsWith(".crdownload")).Select(Path.GetFileName).ToList();
                    if (listTracks2.Any())
                    {
                        string[] separatingString = { ".mp3.crdownload" };
                        try
                        {
                            downloadedTrack = listTracks2.Except(listTracks1).FirstOrDefault().Split(separatingString, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        }
                        catch (Exception)
                        {
                            downloadedTrack = listTracks1.Except(listTracks2).FirstOrDefault().Split(separatingString, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        }
                    }
                }
            }
            catch (Exception)
            {
                YtMp3(chromeDriver, pathFolder, youtubeUrl);
            }
            return downloadedTrack;
        }
        public void RenameFiles(Playlist playlist)
        {
            foreach (var music in playlist.Music)
            {
                if (!string.IsNullOrEmpty(music.NameAfterDownload))
                {
                    string newTrackName = music.Number + " " + RemoveInvalidPathChars(music.Artist + " - " + music.Name + ".mp3");

                    File.Move(playlist.PathFolder + "\\" + music.NameAfterDownload + ".mp3", playlist.PathFolder + "\\" + newTrackName);
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
                int duration = 9;

                var element = chromeDriver.FindElement(By.XPath("//ytd-video-renderer//ytd-thumbnail-overlay-time-status-renderer"), 1);

                string[] time = element.Text.Split(new[] { ":" }, StringSplitOptions.None);

                result = time.Length <= 2 ? true : false;
                if (result)
                {
                    result = int.Parse(time[0]) < duration ? true : false;
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