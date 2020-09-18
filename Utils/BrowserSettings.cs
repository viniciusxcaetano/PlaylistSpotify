using OpenQA.Selenium.Chrome;
using System.Collections.Generic;

namespace PlaylistSpotify.Utils
{
    public static class BrowserSettings
    {
        public static ChromeDriverService chromeDriverService { get; set; }
        public static ChromeOptions chromeOptions { get; set; }

        public static ChromeDriverService ChromeDriverService
        {
            get
            {
                chromeDriverService = ChromeDriverService.CreateDefaultService(); // put the chromedriver.exe in the folder between Program.cs and bin\Debug
                chromeDriverService.HideCommandPromptWindow = true;
                return chromeDriverService;
            }
        }

        public static ChromeOptions ChromeOptions(string pathFolder)
        {
            chromeOptions = new ChromeOptions();
            if (!string.IsNullOrEmpty(pathFolder))
            {
                var downloadPrefs = new Dictionary<string, object>
                {
                {"default_directory", @""+pathFolder+""},
                {"directory_upgrade", true}
                };

                chromeOptions.AddUserProfilePreference("download", downloadPrefs);
            }
            //chromeOptions.AddArgument("headless"); //Not open the browser
            return chromeOptions;
        }
    }
}