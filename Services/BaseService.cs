using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PlaylistSpotify.Models;
using PlaylistSpotify.VideoConverters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace PlaylistSpotify.Services
{
    public static class BaseService
    {
        public static string Download(ChromeDriver chromeDriver, string pathFolder, Music music, int count)
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
                    downloadedTrack = YtMp3.Download(chromeDriver, pathFolder, youtubeUrl);
                    //WaitToDownload(pathFolder);

                    if (!string.IsNullOrEmpty(downloadedTrack))
                    {
                        break;
                    }
                }
            }
            return downloadedTrack;
        }
        public static void WaitToDownload(string pathFolder)
        {
            var tracks = Directory.GetFiles(pathFolder, "*.crdownload").Select(Path.GetFileName).ToList();
            if (tracks.Any())
            {
                Thread.Sleep(2000);
                WaitToDownload(pathFolder);
            }
        }
        public static bool CheckDuration(ChromeDriver chromeDriver)
        {
            bool result = false;
            try
            {
                //check if music duration is less than 9 minutes 
                int duration = 9;

                var element = chromeDriver.FindElement(By.XPath("//ytd-video-renderer//ytd-thumbnail-overlay-time-status-renderer"), 1, 8);
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
        public static string RemoveDoubleSpaces(string name)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(name, " ");
        }
        public static void RenameFiles(Playlist playlist)
        {
            foreach (var music in playlist.Music)
            {
                if (!string.IsNullOrEmpty(music.NameAfterDownload))
                {
                    string newTrackName;
                    if (music.Number < 10)
                    {
                        newTrackName = "0" + music.Number + " " + RemoveInvalidPathChars(music.Artist + " - " + music.Name + ".mp3");
                    }
                    else
                    {
                        newTrackName = music.Number + " " + RemoveInvalidPathChars(music.Artist + " - " + music.Name + ".mp3");
                    }
                    try
                    {
                        File.Move(playlist.PathFolder + "\\" + music.NameAfterDownload + ".mp3", playlist.PathFolder + "\\" + newTrackName);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
        public static string RemoveInvalidPathChars(string name)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(name, "");
        }
        public static Music FormatMusic(Music music)
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
    }
}
