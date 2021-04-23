using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace DMA
{
    class Сentral_Processor
    {
        public static Thread Create_file() =>
            new Thread(() => 
            {
                while (true)
                {
                    if(DMA.request.Equals(false))
                        for (int i = 0; true; i++)
                        {
                            File.Create($"{i}.txt");
                            if (DMA.request.Equals(true))
                            {
                                Thread.Sleep(1000);// Просто посмотреть как работает
                                DMA.proof = true;
                                break;
                            }
                        }
                }
            });      
    }
}
