using System;
using System.Threading;

namespace Server
{
    class Program
    {
        static ServerFunctions server;
        static Thread listenThread;

        static void Main(string[] args)
        {
            try
            {
                server = new ServerFunctions();
                listenThread = new Thread(new ThreadStart(server.ListenConnections));
                listenThread.Start();
            }
            catch (Exception exception)
            {
                server.DisconnectServer();
                Console.WriteLine(exception.Message);
            }
        }
    }
}