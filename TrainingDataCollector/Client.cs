using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Threading;

namespace TrainingDataCollector
{
    
    public class Client
    {
        private const int ALLOWABLE_FAILURES = 10;
        private const int RESET_CYCLES = 600;
        // private TcpClient client;
        //private StreamWriter writer;
        //private StreamReader reader;
        private String username = "DigitDaemon";
        private static String path = Path.Combine(Environment.CurrentDirectory.Replace(@"bin\Debug\netcoreapp2.0", ""), @"Data\");
        private String password = File.ReadAllText(Path.Combine(path, "Token.txt"));//do not push this!!!
        private char[] trimChar = new char[69] { 'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p',
            'q','r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L',
            'M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',' ','.','@','#','!', '_', '-', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'};
        
        public Client()
        {

        }

        public void ClientThread(String channel, int time, ref bool threadEnd, ref System.Timers.Timer trigger)
        {

            Console.WriteLine("thread start");
            TcpClient client = null;
            StreamWriter writer = null;
            StreamReader reader = null;
            StreamWriter fileWriter = null;
            int cycles = 0;
        
            Connect(channel, ref client, ref writer, ref reader, ref fileWriter);


            trigger.Elapsed += (sender, e) => onTick(sender, e, channel, ref client, ref writer, ref reader, ref fileWriter, ref cycles);
            
            while(!threadEnd)
            {
                
            }

            closeThreadResources(ref client, ref writer, ref reader, ref fileWriter);
            
       }

        private void closeThreadResources(ref TcpClient client, ref StreamWriter writer, ref StreamReader reader, ref StreamWriter fileWriter)
        {
            client.Close();
            writer.Close();
            reader.Close();
            fileWriter.Close();
        }

        private void flushThread(string channel, ref TcpClient client, ref StreamWriter writer, ref StreamReader reader, ref StreamWriter fileWriter)
        {
            closeThreadResources(ref client, ref writer, ref reader, ref fileWriter);
            Thread.Sleep(5);
            Connect(channel, ref client, ref writer, ref reader, ref fileWriter);
        }

        private void onTick(Object source, ElapsedEventArgs e, string channel, ref TcpClient client, ref StreamWriter writer, ref StreamReader reader, ref StreamWriter fileWriter, ref int cycles)
        {
            cycles++;

            if (cycles > RESET_CYCLES)
            {
                flushThread(channel, ref client, ref writer, ref reader, ref fileWriter);
                cycles = 0;
            }

            if (!client.Connected)
            {
                closeThreadResources(ref client, ref writer, ref reader, ref fileWriter);
                Connect(channel, ref client, ref writer, ref reader, ref fileWriter);
            }

            if (client.Available > 0 || reader.Peek() >= 0)
            {
                var message = reader.ReadLine();
                var uname = "";
                
                
                message = message.Remove(0, 1);
                if (message.Contains("@") && !message.Contains("JOIN"))
                {
                    while (!message[0].Equals('!'))
                    {
                        uname += message[0];
                        message = message.Remove(0, 1);
                    }

                    message = message.TrimStart(trimChar);
                    message = message.Remove(0, 1);
                    Console.WriteLine(uname + ": " + message);
                    fileWriter.WriteLine(message);
                }
                else
                {
                    Console.WriteLine(message);
                }
            }
            else
            {
                Thread.Sleep(100);
            }

        }

        void Connect(string channel, ref TcpClient client, ref StreamWriter writer, ref StreamReader reader, ref StreamWriter fileWriter)
        {

            int attempts = 0;
            //Connect to twitch irc
            do
            {

                try
                {
                    attempts++;
                    fileWriter = File.AppendText(Path.Combine(path, channel + ".txt"));
                    client = new TcpClient("irc.chat.twitch.tv.", 6667);
                    writer = new StreamWriter(client.GetStream());
                    reader = new StreamReader(client.GetStream());
                    //Log in
                    writer.WriteLine("PASS " + password + Environment.NewLine
                        + "NICK " + username + Environment.NewLine
                        + "USER " + username + " 8 * :" + username);
                    writer.WriteLine("JOIN #" + channel);
                    writer.Flush();

                    break;
                }
                catch (Exception e)
                {
                    if (attempts > ALLOWABLE_FAILURES)
                        throw new Exception("Network Error" + e.Message);
                    else
                        Thread.Sleep(100);
                }
            } while (true);

        }
    }
}
