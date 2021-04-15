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
using System.Security.Principal;
using System.Security.AccessControl;

namespace Service_read_file_parser
{
    public partial class Service_read_file : ServiceBase
    {
        private int eventId = 0;
        public Mutex[] mutex = new Mutex[]
        {
            new Mutex(),
            new Mutex(),
            new Mutex()
        };
        readonly string[] mutex_name = new string[]
        {
            @"Global\File_Id_Text",
            @"Global\File_Id_Href",
            @"Global\File_Id_Image",
            //@"Global\Sync_Processes"
        };
        static readonly string[] path_file = new string[]
        {
            Path.Combine("G:\\","ID_Text_Posts.json"),
            Path.Combine("G:\\","ID_Href_Posts.json"),
            Path.Combine("G:\\","ID_Image_Posts.json")
        };
        static readonly string[] eventwaithandle_name = new string[]
        {
            @"Global\Sync_Aplication",
            @"Global\Sync_Service"
        };
        static public EventWaitHandle[] eventWaitHandle = new EventWaitHandle[2];
        static private int Count_Thread = 0;
        //MemoryMappedFile mmf;
        private readonly System.Timers.Timer timer = new System.Timers.Timer()
        {
            Interval = 5000
        };
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
            eventLogService.WriteEntry("Service start", EventLogEntryType.Information,eventId);
            #region Shared Memory
            //try
            //{
            //    var mappedFileSecurity = new MemoryMappedFileSecurity();
            //    mappedFileSecurity.AddAccessRule(new AccessRule<MemoryMappedFileRights>(
            //        new SecurityIdentifier(WellKnownSidType.WorldSid, null),
            //        MemoryMappedFileRights.FullControl,
            //        AccessControlType.Allow));
            //    mmf = MemoryMappedFile.CreateNew(
            //        "Path_file",
            //        0x100000L,
            //        MemoryMappedFileAccess.ReadWrite,
            //        MemoryMappedFileOptions.None,
            //        mappedFileSecurity,
            //        HandleInheritability.None);
            //    eventLogService.WriteEntry("MMF create", EventLogEntryType.Information, eventId);
            //}
            //catch (Exception ex)
            //{
            //    eventLogService.WriteEntry(ex.Message, EventLogEntryType.Error);
            //}
            #endregion
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }
        protected override void OnStop()
        {
            timer.Stop();
            //mmf.Dispose();
            eventLogService.WriteEntry("Service stop", EventLogEntryType.Information,++eventId);
        }
        private void OnTimer(object sender, ElapsedEventArgs args)
        {
            #region
            //if (Process.GetProcessesByName("Thread-OS").Count().Equals(0))
            //{
            //    eventLogService.WriteEntry("Aplication not start", EventLogEntryType.Warning, eventId);
            //    return;
            //}
            #endregion
            #region Shared Memory
            //try
            //{
            //    using (MemoryMappedViewStream stream = mmf.CreateViewStream())
            //    {
            //        BinaryReader reader = new BinaryReader(stream);
            //        for (int i = 0; i < path_file.Length; i++)
            //        {
            //            path_file[i] = reader.ReadString();
            //            eventLogService.WriteEntry($"Path: {path_file[i]}", EventLogEntryType.Information, eventId);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    eventLogService.WriteEntry(ex.Message, EventLogEntryType.Error, eventId);
            //}
            #endregion
            try
            {
                CheckorCreateEventWaitHandle();
                if (Count_Thread.Equals(0))
                {
                    Count_Thread = 1;
                    eventLogService.WriteEntry("Start read file", EventLogEntryType.Information, ++eventId);

                    for (int i = 0; i < mutex_name.Length; i++)
                    {
                        mutex[i] = new Mutex(false, mutex_name[i]);
                        eventLogService.WriteEntry($"Mutex {mutex_name[i]} find or create", EventLogEntryType.Information, eventId);
                        Thread_Read(mutex[i], path_file[i]).Start();   
                    }
                    eventLogService.WriteEntry($"Start three thread", EventLogEntryType.Information, eventId);
                }
                else if (Count_Thread.Equals(4))
                {
                    if (!Process.GetProcessesByName("Thread-OS").Count().Equals(0))
                    {
                        eventWaitHandle[1].Reset();
                        eventWaitHandle[0].Set();
                    }
                    Count_Thread = 0;
                    eventLogService.WriteEntry($"Finish work threads", EventLogEntryType.Information, eventId);
                }
                else
                {
                    if (Process.GetProcessesByName("Thread-OS").Count().Equals(0))
                    {
                        eventWaitHandle[0].Reset();
                        eventWaitHandle[1].Set();
                        eventLogService.WriteEntry($"Aplication not start", EventLogEntryType.Information, eventId);
                        return;
                    }
                    eventLogService.WriteEntry($"Expect Aplication", EventLogEntryType.Information, eventId);
                    return;
                }  
            }
            catch (Exception ex)
            {
                eventLogService.WriteEntry($"{ex.Message} mutex", EventLogEntryType.Error, eventId);
            }
        }
        private  Thread Thread_Read(Mutex mutex, string path) =>
           new Thread(() =>
           {
               eventWaitHandle[1].WaitOne();
               mutex.WaitOne();
               try
               {
                   if (File.Exists(path))
                       using (StreamReader file = new StreamReader(path))
                       {
                           eventLogService.WriteEntry($"File {path}\n{file.ReadToEnd()}", EventLogEntryType.Information, eventId);
                           file.Close();
                           file.Dispose();
                       }
                   else
                       eventLogService.WriteEntry($"File {path} is not found", EventLogEntryType.Warning, eventId);
               }
               catch (Exception ex)
               {
                   eventLogService.WriteEntry(ex.Message, EventLogEntryType.Error, eventId);
               }
               mutex.ReleaseMutex();
               Count_Thread+=1;
           })
           { Name = $"Read_File{path}" };
        private void CheckorCreateEventWaitHandle()
        {
            if (!EventWaitHandle.TryOpenExisting(eventwaithandle_name[0], out eventWaitHandle[0]))
            {
                EventWaitHandleSecurity eventWaitHandleSecurity = new EventWaitHandleSecurity();
                eventWaitHandleSecurity.AddAccessRule(new EventWaitHandleAccessRule(new SecurityIdentifier("S-1-1-0"),
                    EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify, AccessControlType.Allow));
                eventWaitHandle[0] = new EventWaitHandle(false, EventResetMode.ManualReset, eventwaithandle_name[0], out _, eventWaitHandleSecurity);
            }
            if (!EventWaitHandle.TryOpenExisting(eventwaithandle_name[1], out eventWaitHandle[1]))
            {
                EventWaitHandleSecurity eventWaitHandleSecurity = new EventWaitHandleSecurity();
                eventWaitHandleSecurity.AddAccessRule(new EventWaitHandleAccessRule(new SecurityIdentifier("S-1-1-0"),
                    EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify, AccessControlType.Allow));
                eventWaitHandle[1] = new EventWaitHandle(true, EventResetMode.ManualReset, eventwaithandle_name[1], out _, eventWaitHandleSecurity);
            }
        }
    }
}
