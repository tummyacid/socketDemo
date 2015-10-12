using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketDemo
{
    class Program
    {
        static Socket m_Con;
        static Thread listenThread;
        static void Main(string[] args)
        {
            m_Con = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

            //              m_Con.Connect(System.Net.Dns.Resolve("irc.freenode.net").AddressList[0], 6667);              
            m_Con.Connect(System.Net.IPAddress.Parse("192.40.56.139"), 3133);
            listenThread = new Thread(() =>
            {
                byte[] inData = new byte[10240];
                while (m_Con.Receive(inData, 10240, SocketFlags.None) > 0)
                {
                    String encodedData = Encoding.ASCII.GetString(inData);
                    foreach (String line in encodedData.Split(new string[] { "\r\n"}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string userHost;

                        if (line.Contains("\0"))
                            continue;
                        //Check for a prefix indicator.  If this isnt present the message cannot be processed
                        if (!line.Contains(':'))
                            throw new ArgumentOutOfRangeException("Unable to parse message. " + line);

                        String[] parsedPrefix = line.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0].Split(' ');

                        //Check if prefix is well formed
                        if (parsedPrefix.Count() < 2)
                        {
                            int i = 0;
                            while (RFC1459.commands.Contains(line.Split(':')[++i]   )  )
                            {
                                Console.WriteLine("no command detected");
                                userHost = line.Split(':')[i];
                            }
                            

                        }
                            //else
                            //    throw new ArgumentOutOfRangeException("Unable to parse prefix. " + line);

                        userHost = parsedPrefix[0];
                        string command = parsedPrefix[1];
                        string userName = parsedPrefix[2];
                        string logText = "";

                        //Check if source is blacklisted
                        //if (m_BlackListHosts.Contains(userHost))
                        //{
                        //    //TODO log this?
                        //    return;
                        //}
                        //Examine the command
                        switch (command)
                        {
                            case "PRIVMSG":
                                logText = "PRIVMSG from " + userName;
                                HandlePrivMsg(userName, encodedData.Substring(line.IndexOf(':', 1) + 1)); //skip the first ':' and grab everything after the second
                                break;

                            default:
                                logText = "Unknown Message " + line;
                                break;
                        } 
                    }





                    if (Encoding.ASCII.GetString(inData).Contains("PASS"))
                    {
                        SendString("PASS tummyacid:s");
                        SendString("NICK tummyacid");
                    }
                }   
            });
            listenThread.Start();
            // Convert the string data to byte data using ASCII encoding.
            //SendString("USER tummyacid");
            //SendString("PASS tummyacid:s");
            SendString("USER tummyacid");

            Console.ReadLine();
            listenThread.Abort();
            m_Con.Close();



        }

        private static void HandlePrivMsg(string userName, string p)
        {
            Console.Write(userName + " > " + p + "\r\n");
        }

        private static void SendString(string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data+"\r\n");

            // Begin sending the data to the remote device.
            m_Con.Send(byteData);
        }
    }
}
