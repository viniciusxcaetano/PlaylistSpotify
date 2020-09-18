using OpenQA.Selenium;
using System.Collections.Generic;
using System.Threading;

namespace PlaylistSpotify
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int count)
        {
            try
            {
                Thread.Sleep(2000);
                return driver.FindElement(by);
            }
            catch
            {
                return FindElement(driver, by, 10);
            }
        }

        public static IReadOnlyCollection<IWebElement> FindElements(this IWebDriver driver, By by, int count)
        {
            try
            {
                Thread.Sleep(2000);
                return driver.FindElements(by);
            }
            catch
            {
                return FindElements(driver, by, 10);
            }
        }
    }
}