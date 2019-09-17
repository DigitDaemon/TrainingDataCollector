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

        public void ClientThread(String channel, int time, ref bool threadEnd)
        {

            Console.WriteLine("thread start");
            TcpClient client = null;
            StreamWriter writer = null;
            StreamReader reader = null;
            StreamWriter fileWriter = null;
            Connect(channel, ref client, ref writer, ref reader, ref fileWriter);

            System.Timers.Timer trigger = new System.Timers.Timer(1000);
            trigger.Elapsed += (sender, e) => onTick(sender, e, channel, ref client, ref writer, ref reader, ref fileWriter);
            trigger.AutoReset = true;
            trigger.Enabled = true;

            while(!threadEnd)
            {
                
            }
            
            fileWriter.Close();
            trigger.Dispose();
       }


        private void killThread(Object source, ElapsedEventArgs e, ref bool status)
        {
            status = false;
        }

        private void onTick(Object source, ElapsedEventArgs e, string channel, ref TcpClient client, ref StreamWriter writer, ref StreamReader reader, ref StreamWriter fileWriter)
        {
            
            if (!client.Connected)
            {
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

            //Connect to twitch irc
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

        }
    }
}
