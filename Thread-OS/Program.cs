using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace Thread_OS
{
    class Program
    {
        static List<ID_Text_Post> iD_Text_Posts = new List<ID_Text_Post>();
        static List<ID_Href_Or_Image_Post> iD_Href_Posts = new List<ID_Href_Or_Image_Post>();
        static List<ID_Href_Or_Image_Post> iD_Image_Posts = new List<ID_Href_Or_Image_Post>();

        static object[] lockers = new object[]
        {
            new object(),
            new object(),
            new object()
        };
        static Thread thread_text = new Thread(new ParameterizedThreadStart(Thread_Text_File)) { Name = "Sort_Text" };
        static Thread thread_href = new Thread(new ParameterizedThreadStart(Thread_Href_File)) { Name = "Sort_Href" };
        static Thread thread_image = new Thread(new ParameterizedThreadStart(Thread_Image_File)) { Name = "Sort_Image" };
        static List<IWebElement> feed_row_list;
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

            //for (int i = 0; i < 1; i++)
            //{
                IWebElement feed_rows = null;
                foreach (IWebElement webElement in chromeDriver.FindElementsById("feed_rows").ToList())
                {
                    if (!webElement.Displayed)
                        continue;
                    feed_rows = webElement;
                    break;
                }
                if (feed_rows == null)
                    return;
                 feed_row_list = (from f in feed_rows.FindElements(By.TagName("div"))
                                  where f.Displayed &&
                                  !(f.GetAttribute("class") == null) &&
                                  f.GetAttribute("class").Trim().Equals("feed_row") &&
                                  !f.FindElement(By.TagName("div")).GetAttribute("id").Equals("")
                                  select f).ToList();
                #region
                //Thread thread_Text_file = new Thread(() => WriteinFile("ID_Text_Posts.json", iD_Text_Posts))
                //{
                //    Name = "File_ID_Text"
                //};
                //thread_Text_file.Start();
                //Thread thread_Href_file = new Thread(() => WriteinFile("ID_Href_Posts.json", iD_Href_Posts))
                //{
                //    Name = "File_ID_Href"
                //};
                //thread_Href_file.Start();

                //Thread thread_Image_file = new Thread(() => WriteinFile("ID_Image_Posts.json", iD_Image_Posts))
                //{
                //    Name = "File_ID_Image"
                //};
                //thread_Image_file.Start();
                #endregion
                thread_text.Start(new List<IWebElement>(feed_row_list));
                thread_href.Start(new List<IWebElement>(feed_row_list));
                thread_image.Start(new List<IWebElement>(feed_row_list));
                //hromeDriver.Navigate().Refresh();
            //}
        }
        private static void WriteinFile<T>(string path, List<T> objects)
        {
            using (StreamWriter file = new StreamWriter(path))
            {
                file.WriteLine(JsonConvert.SerializeObject(objects, Formatting.Indented));
                file.Flush();
                file.Close();
                file.Dispose();
            }
        }
        private static void ReadinFile<T>(string path, out List<T> objects)
        {
            using (StreamReader file = new StreamReader(path))
            {
                objects = JsonConvert.DeserializeObject<List<T>>(file.ReadToEnd());
                file.Close();
                file.Dispose();
            }
        }
        private static void Thread_Text_File(object feed_row_list)
            {
                ReadinFile<ID_Text_Post>("ID_Text_Posts.json", out iD_Text_Posts);
                string id_post;
                foreach (IWebElement feed_row in feed_row_list as List<IWebElement>)
                {
                    id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");// FindElement(By.TagName("div")) можно выкинуть
                    if (!iD_Text_Posts.Exists(p => p.Id.Equals(id_post)))
                        continue;
                    try
                    {
                        iD_Text_Posts.Add(new ID_Text_Post(id_post, feed_row.FindElement(By.ClassName("wall_post_text")).Text));
                    }
                    catch (Exception)
                    {
                        iD_Text_Posts.Add(new ID_Text_Post(id_post, null));
                    }
                }
                WriteinFile("ID_Text_Posts.json", iD_Text_Posts);
                iD_Text_Posts.Clear();
            }
        private static void Thread_Href_File(object feed_row_list)
            {
                ReadinFile<ID_Href_Or_Image_Post>("ID_Href_Posts.json", out iD_Href_Posts);
                string id_post;
                foreach (IWebElement feed_row in feed_row_list as List<IWebElement>)
                {
                    id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");// FindElement(By.TagName("div")) можно выкинуть
                    if (!iD_Text_Posts.Exists(p => p.Id.Equals(id_post)))
                        continue;
                    iD_Href_Posts.Add(new ID_Href_Or_Image_Post(id_post, feed_row, "a", "href"));
                }
                WriteinFile("ID_Href_Posts.json", iD_Href_Posts);
                iD_Href_Posts.Clear();
            }
        private static void Thread_Image_File(object feed_row_list)
            {
                ReadinFile<ID_Href_Or_Image_Post>("ID_Image_Posts.json", out iD_Image_Posts);
                string id_post;
                foreach (IWebElement feed_row in feed_row_list as List<IWebElement>)
                {
                    id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");// FindElement(By.TagName("div")) можно выкинуть
                    if (!iD_Text_Posts.Exists(p => p.Id.Equals(id_post)))
                        continue;
                    iD_Image_Posts.Add(new ID_Href_Or_Image_Post(id_post, feed_row, "img", "src", "a", "aria-label"));
                }
                WriteinFile("ID_Image_Posts.json", iD_Image_Posts);
                iD_Image_Posts.Clear();
            }
        private static void Thread_Post(object chromeDriver)
        {
            IWebElement feed_rows = null;
            foreach (IWebElement webElement in (chromeDriver as ChromeDriver).FindElementsById("feed_rows").ToList())
            {
                if (!webElement.Displayed)
                    continue;
                feed_rows = webElement;
                break;
            }
            if (feed_rows == null)
                return;
            feed_row_list = (from f in feed_rows.FindElements(By.TagName("div"))
                             where f.Displayed &&
                             !(f.GetAttribute("class") == null) &&
                             f.GetAttribute("class").Trim().Equals("feed_row") &&
                             !f.FindElement(By.TagName("div")).GetAttribute("id").Equals("")
                             select f).ToList();
        }
        private static void Thread_()
            {

            }
            //{Name="Write_File" };
    }
}
