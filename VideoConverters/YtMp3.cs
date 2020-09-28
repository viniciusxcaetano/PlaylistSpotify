using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PlaylistSpotify.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlaylistSpotify.VideoConverters
{
    public static class YtMp3
    {
        private static IWebElement WebElement { get; set; }
        public static string Download(ChromeDriver chromeDriver, string pathFolder, string youtubeUrl)
        {
            string downloadedTrack = "";
            try
            {
                chromeDriver.Navigate().GoToUrl("https://ytmp3.cc/en13/");
                WebElement = chromeDriver.FindElement(By.Id("input"), 1, 8);
                WebElement.SendKeys(youtubeUrl);
                WebElement = chromeDriver.FindElement(By.Id("submit"), 1, 8);
                WebElement.Click();
                var listTracks1 = Directory.GetFiles(pathFolder, "*").Where(s => s.EndsWith(".crdownload")).Select(Path.GetFileName).ToList();
                WebElement = chromeDriver.FindElement(By.XPath("//a[contains(text(),'Download')]"), 1, 100);

                if (WebElement == null)
                {
                    return "";
                }
                BaseService.WaitToDownload(pathFolder);
                WebElement.Click();
                List<string> tabs = new List<string>(chromeDriver.WindowHandles);

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
                //CheckIfDownloadedAll(pathFolder);
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2147024864)
                {
                    Download(chromeDriver, pathFolder, youtubeUrl);
                }
            }
            return downloadedTrack;
        }
    }
}
