using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace peripheral_device
{
    class Program
    {
        static readonly string File_path = "DMA.json";
        static readonly string Mutex_name = "DMA_file";
        static readonly string[] eventwaithandle_name = new string[]
        {
            "DMA",
            "Pepheral device"
        };
        static public EventWaitHandle[] eventWaitHandle = new EventWaitHandle[2];
        static void Main(string[] args)
        {
            while (true)
            {
                CheckorCreateEventWaitHandle();
                eventWaitHandle[1].WaitOne();
                Mutex mutex = new Mutex(false, Mutex_name);
                mutex.WaitOne();
                var table = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(File_path));
                foreach (var elem in table)
                {
                    switch (elem.Code_operation)
                    {
                        case (0):
                            Console.WriteLine("Введите данные с клавиатуры");
                            elem["Result"] = Console.ReadLine();
                            break;
                        case (1):
                            Console.WriteLine($"Печать с принтера: {elem.Result}");
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
