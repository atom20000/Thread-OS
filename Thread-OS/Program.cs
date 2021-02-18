using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
namespace Thread_OS
{
    class Program
    {
        static void Main(string[] args)
        {
            PathBrowserUserData pathBrowserUserData = new PathBrowserUserData("config.json");
            ChromeOptions options = new ChromeOptions
            {
                BinaryLocation = pathBrowserUserData.BrowserPath,
                LeaveBrowserRunning = true
            };
            options.AddArgument(pathBrowserUserData.User_DataPath);
            ChromeDriver chromeDriver = new ChromeDriver(options);
            chromeDriver.Navigate().GoToUrl("https://vk.com/feed");
            IWebElement feed_rows = null;
            foreach( IWebElement webElement in chromeDriver.FindElementsById("feed_rows").ToList())
            {
                if (!webElement.Displayed)
                    continue;
                feed_rows = webElement;
            }
            if (feed_rows == null)
                return;
            List<ID_Text_Post> iD_Text_Posts = new List<ID_Text_Post>();
            List<ID_Href_Or_Image_Post> iD_Href_Posts = new List<ID_Href_Or_Image_Post>();
            List<ID_Href_Or_Image_Post> iD_Image_Posts = new List<ID_Href_Or_Image_Post>();
            List<string> Href = new List<string>();
            List<string> Image = new List<string>();
            string id_post;
            string url;
            foreach (IWebElement feed_row in feed_rows.FindElements(By.TagName("div")))
            {
                if (!feed_row.Displayed)
                    continue;
                if (feed_row.GetAttribute("class") == null)
                    continue;
                if (!feed_row.GetAttribute("class").Trim().Equals("feed_row"))
                    continue;
                id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");
                try
                {
                    iD_Text_Posts.Add(new ID_Text_Post(id_post, feed_row.FindElement(By.ClassName("wall_post_text")).Text));
                }
                catch (Exception)
                {
                    iD_Text_Posts.Add(new ID_Text_Post(id_post, null));
                }
                Href.Clear();
                foreach (IWebElement href in feed_row.FindElements(By.TagName("a")).ToList())
                {
                    if (href.GetAttribute("href") == null || href.GetAttribute("href") == "")
                        continue;
                    if (Href.Exists(p => p.Equals(href.GetAttribute("href"))))
                        continue;
                    Href.Add(href.GetAttribute("href"));
                }   
                iD_Href_Posts.Add(new ID_Href_Or_Image_Post(id_post, Href));
                Image.Clear();
                foreach (IWebElement image in feed_row.FindElements(By.TagName("img")).ToList())
                {
                    if (image.GetAttribute("src") == null || image.GetAttribute("src") == "")
                        continue;
                    if (Image.Exists(p => p.Equals(image.GetAttribute("src"))))
                        continue;
                    Image.Add(image.GetAttribute("src"));
                }
                foreach (IWebElement image in feed_row.FindElements(By.TagName("a")).ToList())
                {
                    if (image.GetAttribute("aria-label") == null || image.GetAttribute("aria-label") == "")
                        continue;
                    if (!image.GetAttribute("aria-label").Equals("фотография"))
                        continue;
                    url = image.GetAttribute("style");
                    url = url.Substring(url.IndexOf('"') + 1);
                    url = url.Remove(url.IndexOf('"'));
                    if (Image.Exists(p => p.Equals(url)))
                        continue;
                    Image.Add(url);
                }
                iD_Image_Posts.Add(new ID_Href_Or_Image_Post(id_post, Image));
            }
            System.IO.File.WriteAllText("ID_Text_Posts.json", JsonConvert.SerializeObject(iD_Text_Posts, Formatting.Indented));
            System.IO.File.WriteAllText("ID_Href_Posts.json", JsonConvert.SerializeObject(iD_Href_Posts, Formatting.Indented));
            System.IO.File.WriteAllText("ID_Image_Posts.json", JsonConvert.SerializeObject(iD_Image_Posts, Formatting.Indented));
        }
    }
}
