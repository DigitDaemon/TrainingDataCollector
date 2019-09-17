using System;
using System.Threading;


namespace TrainingDataCollector
{
    class Program
    {
        static void Main(string[] args)
        {


            Client cl = new Client();
            bool thrEnd = false;

            foreach (string channel in args)
            {
                Thread thr = new Thread(() => cl.ClientThread(channel, 60, ref thrEnd));
                thr.Start();
            }



            //Thread.Sleep(duration * 1000);
            Console.ReadLine();
            thrEnd = true;
            
            
        }

    }
}
