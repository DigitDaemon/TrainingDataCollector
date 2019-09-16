using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace TrainingDataCollector
{
    
    class Client
    {
        private TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;
        private String username;
        private String password;
        private char[] trimChar;
        private Timer timer;
        
        public Client()
        {

        }

        public void run()
        {
            setTimer();

            
        }

        private void setTimer()
        {
            timer = new Timer(1000);

            timer.Elapsed += onTick;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void onTick(Object source, ElapsedEventArgs e)
        {
            if (!client.Connected)
            {
                Connect();
            }

            if (client.Available > 0 || reader.Peek() >= 0)
            {
                var message = reader.ReadLine();
                var uname = "";

                message.Remove(0, 1);
                while (!message[0].Equals("!"))
                {
                    uname += message[0];
                    message.Remove(0, 1);
                }

                message.TrimStart(':');
                message.Remove(0, 1);

                Console.WriteLine(uname + ": " + message);
            }

        }

        void Connect()
        {
            this.username = "";
            this.password = ""; //do not push this!!!

            //Connect to twitch irc
            client = new TcpClient("irc.chat.twitch.tv.", 6667);
            writer = new StreamWriter(client.GetStream());
            reader = new StreamReader(client.GetStream());
            //Log in
            writer.WriteLine("PASS " + password + Environment.NewLine
                + "NICK " + username + Environment.NewLine
                + "USER " + username + " 8 * :" + username);
            writer.WriteLine("JOIN #rigusstorm");
            writer.Flush();

        }
    }
}
