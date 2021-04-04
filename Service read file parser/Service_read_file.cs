using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Timers;

namespace Service_read_file_parser
{
    public partial class Service_read_file : ServiceBase
    {
        private int eventId = 1;
        public Mutex[] mutex = new Mutex[]
        {
            new Mutex(),
            new Mutex(),
            new Mutex()
        };
        readonly string[] mutex_name = new string[]
        {
            @"Global\ File_Id_Text",
            @"Global\ File_Id_Href",
            @"Global\ File_Id_Image"
        };
        string[] path_file = new string[]
        {
            @"G:\Репозитории\Thread-OS\Thread-OS\bin\Debug\netcoreapp3.1\ID_Text_Posts.json",
            @"G:\Репозитории\Thread-OS\Thread-OS\bin\Debug\netcoreapp3.1\ID_Href_Posts.json",
            @"G:\Репозитории\Thread-OS\Thread-OS\bin\Debug\netcoreapp3.1\ID_Image_Posts.json"
        };
        //static Thread[] threads = new Thread[3];
        //static Thread thread_text;
        //static Thread thread_href;
        //static Thread thread_image;
        public Service_read_file()
        {
            InitializeComponent();
            eventLogService = new EventLog();
            if (!EventLog.SourceExists("Parser VK Service"))
            {
                EventLog.CreateEventSource("Parser VK Service", "Parser VK Service");
            }
            eventLogService.Source = "Parser VK Service";
            eventLogService.Log = "Parser VK Service";
        }

        protected override void OnStart(string[] args)
        {
            eventLogService.WriteEntry("Service start", EventLogEntryType.Information);
            #region Shared Memory
            //try
            //{
            //    using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting(@"Global\ Path_file"))
            //    {
            //        using (MemoryMappedViewStream stream = mmf.CreateViewStream())
            //        {
            //            BinaryReader reader = new BinaryReader(stream);
            //            for(int i = 0; i < path_file.Length; i++)
            //            {
            //                path_file[i] = reader.ReadString();
            //                eventLogService.WriteEntry(path_file[i], EventLogEntryType.Information,eventId);
            //            }          
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    eventLogService.WriteEntry(ex.Message, EventLogEntryType.Error);
            //}
            #endregion
            System.Timers.Timer timer = new System.Timers.Timer()
            {
                Interval = 5000               
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
            eventLogService.WriteEntry("Service stop", EventLogEntryType.Information,eventId++);
        }
        private void OnTimer(object sender, ElapsedEventArgs args)
        { 
            eventLogService.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
            if (Process.GetProcessesByName("Thread-OS").Count().Equals(0))
                return;
            try
            {
                for(int i = 0; i < mutex_name.Length; i++)
                {
                    mutex[i] = Mutex.OpenExisting(mutex_name[i]);
                    //eventLogService.WriteEntry(mutex[i].ToString(), EventLogEntryType.Information, eventId);// удалить
                    Thread_Read(mutex[i], path_file[i]).Start();
                }  
            }
            catch (Exception ex)
            {
                eventLogService.WriteEntry(ex.Message, EventLogEntryType.Error, eventId);
            }
        }
        private  Thread Thread_Read(Mutex mutex, string path) =>
           new Thread(() =>
           {
               mutex.WaitOne();
               try
               {
                   if (File.Exists(path))
                       using (StreamReader file = new StreamReader(path))
                       {
                           eventLogService.WriteEntry(file.ReadToEnd(), EventLogEntryType.Information, eventId);
                           file.Close();
                           file.Dispose();
                       }
               }
               catch (Exception ex)
               {
                   eventLogService.WriteEntry(ex.Message, EventLogEntryType.Error, eventId);
               }
               mutex.ReleaseMutex();
           })
           { Name = $"Read_File{path}" };
    }
}
