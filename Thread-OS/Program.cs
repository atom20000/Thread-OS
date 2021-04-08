using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.ServiceProcess;

namespace Thread_OS
{
    class Program
    {
        static public readonly Mutex[] mutex = new Mutex[]
        {
            new Mutex(false,@"Global\ File_Id_Text"),
            new Mutex(false,@"Global\ File_Id_Href"),
            new Mutex(false,@"Global\ File_Id_Image")
        };
        static readonly object[] locker = new object[]
        {
            new object(),
            new object(),
            new object()
        };
        static readonly string[] path_file = new string[]
        {
            Path.Combine(Directory.GetCurrentDirectory(),"ID_Text_Posts.json"),
            Path.Combine(Directory.GetCurrentDirectory(),"ID_Href_Posts.json"),
            Path.Combine(Directory.GetCurrentDirectory(),"ID_Image_Posts.json")
        };
        static Thread thread_text;
        static Thread thread_href;
        static Thread thread_image;
        static Thread thread_post;
        static Thread thread_read;

        static void Main(string[] args)
        {
            //StartService("Service_read_file");
            SharedMemory();
            #region Подключение хром драйвера
            PathBrowserUserData pathBrowserUserData = new PathBrowserUserData("config.json");
            ChromeOptions options = new ChromeOptions
            {
                BinaryLocation = pathBrowserUserData.BrowserPath,
                LeaveBrowserRunning = true
            };
            options.AddArgument(pathBrowserUserData.User_DataPath);
            try
            {
                ChromeDriver chromeDriver = new ChromeDriver(options);
                chromeDriver.Navigate().GoToUrl("https://vk.com/feed");
                #endregion
                for (int i = 0; i < 2; i++)
                {
                    SharedMemory();
                    thread_post = Thread_Post(chromeDriver);
                    thread_post.Start();
                    thread_post.Join();
                    //chromeDriver.Navigate().Refresh();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n Скорее всего браузер не был закрыт");
                return;
            }
            //StopService("Service_read_file");
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
            if (File.Exists(path))
                using (StreamReader file = new StreamReader(path))
                {
                    objects = JsonConvert.DeserializeObject<List<T>>(file.ReadToEnd());
                    file.Close();
                    file.Dispose();
                }
            else
                objects = new List<T>();
        }
        private static Thread Thread_Text_File(object feed_row_list) =>
            new Thread(() =>
            {
                List<ID_Text_Post> iD_Text_Posts;
                lock (locker[0])
                {   
                    ReadinFile<ID_Text_Post>(path_file[0], out iD_Text_Posts);
                    string id_post;
                    foreach (IWebElement feed_row in feed_row_list as List<IWebElement>)
                    {
                        id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");// FindElement(By.TagName("div")) можно выкинуть
                        if (iD_Text_Posts.Exists(p => p.Id.Equals(id_post)))
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
                }
                mutex[0].WaitOne();
                WriteinFile("ID_Text_Posts.json", new List<ID_Text_Post>(iD_Text_Posts));
                mutex[0].ReleaseMutex();
                iD_Text_Posts.Clear();               
            })
            { Name = "Sort_Text" };
        private static Thread Thread_Href_File(object feed_row_list)=>
            new Thread(()=>
            {    
                List<ID_Href_Or_Image_Post> iD_Href_Posts;
                lock (locker[1])
                {
                    ReadinFile<ID_Href_Or_Image_Post>(path_file[1], out iD_Href_Posts);
                    string id_post;
                    foreach (IWebElement feed_row in feed_row_list as List<IWebElement>)
                    {
                        id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");// FindElement(By.TagName("div")) можно выкинуть
                        if (iD_Href_Posts.Exists(p => p.Id.Equals(id_post)))
                            continue;
                        iD_Href_Posts.Add(new ID_Href_Or_Image_Post(id_post, feed_row, "a", "href"));
                    }
                }
                mutex[1].WaitOne();
                WriteinFile("ID_Href_Posts.json", new List < ID_Href_Or_Image_Post >(iD_Href_Posts));
                mutex[1].ReleaseMutex();
                iD_Href_Posts.Clear();             
            })
            { Name = "Sort_Href" };
        private static Thread Thread_Image_File(object feed_row_list)=>
            new Thread(()=>
            {  
                List<ID_Href_Or_Image_Post> iD_Image_Posts;
                lock (locker[2])
                {
                    ReadinFile<ID_Href_Or_Image_Post>(path_file[2], out iD_Image_Posts);
                    string id_post;
                    foreach (IWebElement feed_row in feed_row_list as List<IWebElement>)
                    {
                        id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");// FindElement(By.TagName("div")) можно выкинуть
                        if (iD_Image_Posts.Exists(p => p.Id.Equals(id_post)))
                            continue;
                        iD_Image_Posts.Add(new ID_Href_Or_Image_Post(id_post, feed_row, "img", "src", "a", "aria-label"));
                    }
                }
                mutex[2].WaitOne();
                WriteinFile("ID_Image_Posts.json", new List<ID_Href_Or_Image_Post>(iD_Image_Posts));
                mutex[2].ReleaseMutex();
                iD_Image_Posts.Clear();
            })
            { Name = "Sort_Image" };
        private static Thread Thread_Post(object chromeDriver)=>
            new Thread(()=>
            {
                thread_read = Thread_Read();
                thread_read.Start();
                IWebElement feed_rows = null;
                List<IWebElement> feed_row_list;
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
                thread_text = Thread_Text_File(new List<IWebElement>(feed_row_list));
                thread_text.Start();
                thread_href = Thread_Href_File(new List<IWebElement>(feed_row_list));
                thread_href.Start();
                thread_image = Thread_Image_File(new List<IWebElement>(feed_row_list));
                thread_image.Start();
                //thread_text.Join();
                //thread_href.Join();
                //thread_image.Join();
                lock(locker[0])
                    lock(locker[1])
                        lock(locker[2])
                            (chromeDriver as ChromeDriver).Navigate().Refresh();
        })
        { Name = "Find_Post" };
        private static Thread Thread_Read()=>
            new Thread(()=>
            {
                //mutex[0].WaitOne();
                //mutex[1].WaitOne();
                //mutex[2].WaitOne();
                for (int i = 0; i<3; i++) //foreach (string path in path_file)
                {
                    mutex[i].WaitOne();
                    Console.WriteLine($"Данные из документа {path_file[i]}");
                    if (File.Exists(path_file[i]))
                        using (StreamReader file = new StreamReader(path_file[i]))
                        {
                            Console.WriteLine(file.ReadToEnd());
                            file.Close();
                            file.Dispose();
                        }
                    mutex[i].ReleaseMutex();
                }
                //mutex[0].ReleaseMutex();
                //mutex[1].ReleaseMutex();
                //mutex[2].ReleaseMutex();
            })
            { Name = "Read_File" };
        private static void StartService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
                return;
            service.Start();
        }
        private static void StopService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            if(service.Status != ServiceControllerStatus.Stopped)
                service.Stop();
        }
        private static void SharedMemory()
        {
            if (!new ServiceController("Service_read_file").Status.Equals(ServiceControllerStatus.Running))
                return;
            using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(@"Global\Path_file"))
            {
                using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    foreach (string path in path_file)
                    {
                        writer.Write(path);
                    }
                }
            }
        }
    }
}
