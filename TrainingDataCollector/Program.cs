using System;
using System.Threading;
using System.Timers;

namespace TrainingDataCollector
{
    class Program
    {
        static Client cl;
        static bool thrEnd;
        static System.Timers.Timer trigger;

        static void Main(string[] args)
        {
            cl = new Client();
            thrEnd = false;
            trigger = new System.Timers.Timer(1000);
            trigger.AutoReset = true;
            trigger.Enabled = true;

            foreach (string channel in args)
            {
                Thread thr = new Thread(() => cl.ClientThread(channel, 60, ref thrEnd, ref trigger));
                thr.Start();
            }

            

            
            Console.ReadLine();
            trigger.Dispose();
            thrEnd = true;
            
            
        }

    }
}
