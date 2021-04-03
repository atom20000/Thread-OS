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
using System.Timers;

namespace Service_read_file_parser
{
    public partial class Service_read_file : ServiceBase
    {
        private int eventId = 1;
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
            System.Timers.Timer timer = new System.Timers.Timer()
            {
                Interval = 1000               
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
            eventLogService.WriteEntry("Service stop", EventLogEntryType.Information);
        }
        private void OnTimer(object sender, ElapsedEventArgs args)
        { 

            eventLogService.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
            try
            {
                eventLogService.WriteEntry(Mutex.OpenExisting("File_Id_Text").ToString(), EventLogEntryType.Information, eventId); 
            }
            catch (Exception ex)
            {
                eventLogService.WriteEntry(ex.Message, EventLogEntryType.Error, eventId);
            }
        }
           
    }
}
