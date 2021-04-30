using System;
using System.IO;
using System.Dynamic;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;

namespace DMA
{
    class Program
    {
        static readonly string File_path= @"G:\Репозитории\Thread-OS\DMA.json";
        static readonly string Mutex_name = "DMA_file";
        static readonly string[] eventwaithandle_name = new string[]
        {
            "DMA",
            "Pepheral device"
        };
        static public EventWaitHandle[] eventWaitHandle = new EventWaitHandle[2];
        static Action<Mutex> CheckAbandonedMutex = new Action<Mutex>((mutex) =>
        {
            try
            {
                mutex.WaitOne();
            }
            catch (AbandonedMutexException)
            {
                mutex.ReleaseMutex();
                mutex.WaitOne();
            }
        });
        static System.Timers.Timer timer = new System.Timers.Timer()
        {
            Interval = 5000
        };
        static void Main(string[] args)
        {
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);
            if (Process.GetProcessesByName("peripheral device").Count().Equals(0))
            {
                Console.WriteLine("Запустите перефирийные устройства");
                Console.ReadLine();
                return;
            }
            if (!File.Exists(File_path))
                File.Create(File_path).Close();
            File.WriteAllText(File_path, JsonConvert.SerializeObject(null, Formatting.Indented));
            for (int i = 0; Menu(i); i++)
            {
                if (Process.GetProcessesByName("peripheral device").Count().Equals(0))
                {
                    Console.WriteLine("Запустите перефирийные устройства");
                    Console.ReadLine();
                    return;
                }
            }
        }
        static bool Menu(int Id_process)
        {
            Console.WriteLine("1. Клавиатура \n2. Принтер \n3. Выход");
            int way;
            if (!int.TryParse(Console.ReadLine(), out way))
                return true;
            switch (way)
            {
                case (1):
                    Keyboard(Id_process);
                    break;
                case (2):
                    Printer(Id_process);
                    break;
                case (3):
                    return false;

            }
            return true;
        }
        static void Keyboard(object Id_process)
        {
            //Добавить мьютексы
            Mutex mutex = new Mutex(false, Mutex_name);
            CheckorCreateEventWaitHandle();
            eventWaitHandle[0].WaitOne();
            CheckAbandonedMutex(mutex);
            List<JToken> table = JsonConvert.DeserializeObject<List<JToken>>(File.ReadAllText(File_path));
            if (table == null)
                table = new List<JToken>();
            table.Add( new JObject
            {
                {"Process_id", (int)Id_process},
                {"Code_operation", 0},
                {"Result", null}
                //МБ добавить bool для подтверждения
            }
            );
            File.WriteAllText(File_path, JsonConvert.SerializeObject(table, Formatting.Indented));
            mutex.ReleaseMutex();
            eventWaitHandle[0].Reset();
            eventWaitHandle[1].Set();
            timer.Start();
            eventWaitHandle[0].WaitOne();
            timer.Stop();
            CheckAbandonedMutex(mutex);
            table = JsonConvert.DeserializeObject<List<JToken>>(File.ReadAllText(File_path));
            Console.WriteLine($"Id process:{Id_process} вывод {table.Find(el => el.Children().First().Children().First().ToObject<int>().Equals(Id_process)).ElementAt(2).Children().First().ToString()}");
            table.RemoveAll(el => el.Children().First().Children().First().ToObject<int>().Equals(Id_process));
            File.WriteAllText(File_path, JsonConvert.SerializeObject(table, Formatting.Indented));
            mutex.ReleaseMutex();
            //Добавить ожидание выполнение завершения процесса и удаление записи из файла и вывод на экран результата
        }
        static void Printer(object Id_process)
        {
            //Добавить мьютексы
            Mutex mutex = new Mutex(false, Mutex_name);
            CheckorCreateEventWaitHandle();
            eventWaitHandle[0].WaitOne();
            CheckAbandonedMutex(mutex);
            List<JToken> table = JsonConvert.DeserializeObject<List<JToken>>(File.ReadAllText(File_path));
            if (table == null)
                table = new List<JToken>();
            Console.WriteLine("Что печатаем?\n");
            table.Add(new JObject
            {
                {"Process_id", (int)Id_process},
                {"Code_operation", 1},
                {"Result", Console.ReadLine()}
                //МБ добавить bool для подтверждения
            }
            );
            File.WriteAllText(File_path, JsonConvert.SerializeObject(table, Formatting.Indented));
            mutex.ReleaseMutex();
            eventWaitHandle[0].Reset();
            eventWaitHandle[1].Set();
            timer.Start();
            eventWaitHandle[0].WaitOne();
            timer.Stop();
            CheckAbandonedMutex(mutex);
            table = JsonConvert.DeserializeObject<List<JToken>>(File.ReadAllText(File_path));
            Console.WriteLine($"Id process:{Id_process} Печать завершена");
            table.RemoveAll(el => el.Children().First().Children().First().ToObject<int>().Equals(Id_process));
            File.WriteAllText(File_path, JsonConvert.SerializeObject(table, Formatting.Indented));
            mutex.ReleaseMutex();
            //Добавить ожидание выполнение завершения процесса и удаление записи из файла
        }
        private static void CheckorCreateEventWaitHandle()
        {
            if (!EventWaitHandle.TryOpenExisting(eventwaithandle_name[0], out eventWaitHandle[0]))
                eventWaitHandle[0] = new EventWaitHandle(true, EventResetMode.ManualReset, eventwaithandle_name[0]);
            if (!EventWaitHandle.TryOpenExisting(eventwaithandle_name[1], out eventWaitHandle[1]))
                eventWaitHandle[1] = new EventWaitHandle(false, EventResetMode.ManualReset, eventwaithandle_name[1]);
        }
        private static void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (Process.GetProcessesByName("peripheral device").Count().Equals(0))
            {
                eventWaitHandle[1].Reset();
                eventWaitHandle[0].Set();
            }
        }
    }
}
