using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace Thread_OS
{
    class PathBrowserUserData
    {
        public string BrowserPath;
        public string User_DataPath;
        /// <summary>
        /// Создает объект из Json-файла с названием name 
        /// </summary>
        /// <param name="name"></param>
        public PathBrowserUserData(string name)
        {
            string FileText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), name));
            this.BrowserPath = JsonConvert.DeserializeObject<PathBrowserUserData>(FileText).BrowserPath;
            this.User_DataPath = JsonConvert.DeserializeObject<PathBrowserUserData>(FileText).User_DataPath;
        }
        public PathBrowserUserData() { }
        /// <summary>
        /// Сохраняет объект в Json-файл с названием name 
        /// </summary>
        /// <param name="name"></param>
        public void ToJsonFile(string name)=>
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), name), JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
