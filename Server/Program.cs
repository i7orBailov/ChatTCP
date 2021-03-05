using System;
using System.Threading;

namespace Server
{
    class Program
    {
        static ServerFunctions server;
        static Thread listenThread;
        static Thread showStatusThread;

        static void Main(string[] args)
        {
            try
            {
                server = new ServerFunctions();
                listenThread = new Thread(new ThreadStart(server.ListenConnections));
                listenThread.Start();
                showStatusThread = new Thread(new ThreadStart(server.ShowInfo));
                showStatusThread.Start();
            }
            catch (Exception exception)
            {
                server.DisconnectServer();
                Console.WriteLine(exception.Message);
            }
        }
    }
}