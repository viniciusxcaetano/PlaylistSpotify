using OpenQA.Selenium;
using System.Collections.Generic;
using System.Threading;

namespace PlaylistSpotify
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int count, int times)
        {
            if (count < times)
            {
                try
                {
                    Thread.Sleep(2000);
                    return driver.FindElement(by);
                }
                catch
                {
                    count++;
                    return FindElement(driver, by, count, times);
                }
            }
            return null;
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
                return FindElements(driver, by, count);
            }
        }
    }
}