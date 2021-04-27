using System;
using System.IO;
using System.Dynamic;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace DMA
{
    class Program
    {
        static readonly string File_path="DMA.json";
        static readonly string Mutex_name = "DMA_file";
        static readonly string[] eventwaithandle_name = new string[]
        {
            "DMA",
            "Pepheral device"
        };
        static public EventWaitHandle[] eventWaitHandle = new EventWaitHandle[2];
        static void Main(string[] args)
        {
            for (int i = -1; Menu(i); i++);
        }
        static bool Menu(int Id_process)
        {
            Console.WriteLine("1. Клавиатура \n2. Принтер \n3.Выход");
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
            mutex.WaitOne();
            var table = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(File_path));
            table.Add( new JObject
            {
                {"Process_id", (int)Id_process},
                {"Code_operation", 0},
                {"Result", null}
            }
            );
            File.WriteAllText(File_path, JsonConvert.SerializeObject(table, Formatting.Indented));
            mutex.ReleaseMutex();
            eventWaitHandle[0].Reset();
            eventWaitHandle[1].Set();
            eventWaitHandle[0].WaitOne();
            mutex.WaitOne();
            table = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(File_path));
            //table.ToList().Find(el => el.Process_id.Equals(Id_process));
            //Добавить ожидание выполнение завершения процесса и удаление записи из файла и вывод на экран результата
        }
        static void Printer(object Id_process)
        {
            //Добавить мьютексы
            Mutex mutex = new Mutex(false, Mutex_name);
            CheckorCreateEventWaitHandle();
            eventWaitHandle[0].WaitOne();
            mutex.WaitOne();
            var table = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(File_path));
            table.Add(new JObject
            {
                {"Process_id", (int)Id_process},
                {"Code_operation", 1},
                {"Result", Console.ReadLine()}
            }
            );
            File.WriteAllText(File_path, JsonConvert.SerializeObject(table, Formatting.Indented));
            mutex.ReleaseMutex();
            eventWaitHandle[0].Reset();
            eventWaitHandle[1].Set();
            eventWaitHandle[0].WaitOne();
            mutex.WaitOne();
            table = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(File_path));
            //Добавить ожидание выполнение завершения процесса и удаление записи из файла
        }
        private static void CheckorCreateEventWaitHandle()
        {
            if (!EventWaitHandle.TryOpenExisting(eventwaithandle_name[0], out eventWaitHandle[0]))
                eventWaitHandle[0] = new EventWaitHandle(true, EventResetMode.ManualReset, eventwaithandle_name[0]);
            if (!EventWaitHandle.TryOpenExisting(eventwaithandle_name[1], out eventWaitHandle[1]))
                eventWaitHandle[1] = new EventWaitHandle(false, EventResetMode.ManualReset, eventwaithandle_name[1]);
        }
    }
}
