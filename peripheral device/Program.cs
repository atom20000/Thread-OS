using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace peripheral_device
{
    class Program
    {
        static readonly string File_path = @"C:\Users\USER\Desktop\Thread-OS\DMA.json";
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
        static void Main(string[] args)
        {
            while (true)
            {
                CheckorCreateEventWaitHandle();
                eventWaitHandle[1].WaitOne();
                Mutex mutex = new Mutex(false, Mutex_name);
                CheckAbandonedMutex(mutex);
                if (!File.Exists(File_path))
                    File.Create(File_path).Close();
                List<JToken> table = JsonConvert.DeserializeObject<List<JToken>>(File.ReadAllText(File_path));
                if (table == null)
                    table = new List<JToken>();
                foreach (var elem in table)
                {
                    switch (elem.Children().ElementAt(1).Children().First().ToObject<int>())
                    {
                        case (0):
                            Console.WriteLine("Введите данные с клавиатуры");
                            elem["Result"] = Console.ReadLine();
                            break;
                        case (1):
                            Console.WriteLine($"Печать с принтера: {elem.Children().ElementAt(2).First().ToString()}");
                            break;
                    }  
                }
                File.WriteAllText(File_path, JsonConvert.SerializeObject(table, Formatting.Indented));
                mutex.ReleaseMutex();
                eventWaitHandle[1].Reset();
                eventWaitHandle[0].Set();
            }
        }
        static void Keyboard()
        {
            //Добавить мьютексы
            var table = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(File_path));

            File.WriteAllText(File_path, JsonConvert.SerializeObject(table, Formatting.Indented));
            //Добавить ожидание выполнение завершения процесса и удаление записи из файла
        }
        static void Printer()
        {
            //Добавить мьютексы
            var table = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(File_path));

            File.WriteAllText(File_path, JsonConvert.SerializeObject(table, Formatting.Indented));
            //Добавить ожидание выполнение завершения процесса и удаление записи из файла
        }
        private static void CheckorCreateEventWaitHandle()
        {
            if (!EventWaitHandle.TryOpenExisting(eventwaithandle_name[0], out eventWaitHandle[0]))
                eventWaitHandle[0] = new EventWaitHandle(false, EventResetMode.ManualReset, eventwaithandle_name[0]);
            if (!EventWaitHandle.TryOpenExisting(eventwaithandle_name[1], out eventWaitHandle[1]))
                eventWaitHandle[1] = new EventWaitHandle(true, EventResetMode.ManualReset, eventwaithandle_name[1]);
        }
    }
}
