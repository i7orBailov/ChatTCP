using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class ServerFunctions
    {
        static TcpListener server = new TcpListener(IPAddress.Any, 8080);
        List<UserInstance> connectedUsers = new List<UserInstance>(); // TODO : later use DateBase instead

        protected internal void ListenConnections()
        {
            server = new TcpListener(IPAddress.Any, 8080);
            server.Start();

            Console.WriteLine("Server is active. Waiting for connections...");

            while (true)
            {
                var tcpClient = server.AcceptTcpClient();

                var user = new UserInstance(tcpClient, this);
                var userAuthorization = new UserAuthorization(user, user.toRegister);
                ConnectUser(user);
                var clientThread = new Thread(new ThreadStart(user.BroadcastMessageToChat));
                clientThread.Start();
            }
        }

        protected internal void ConnectUser(UserInstance userToConnect)
        {
            connectedUsers.Add(userToConnect);

            string messageAboutJoining = $"{DateTime.Now.ToShortTimeString()}" +
                    $" : {userToConnect.userNickName} connected to the server.";


            
            // TODO : clause for server : user 'logged'/'registered' and then connected to the server

            NotifyAllUsers(messageAboutJoining, userToConnect.userID);
            NotifyServer(messageAboutJoining, userJoined: true);
        }
        
        protected internal void NotifyAllUsers(string messageToSend, int senderUserID, bool includeSender = false)
        {
            byte[] writeBuffer = Encoding.Unicode.GetBytes(messageToSend);

            for (int i = 0; i < connectedUsers.Count; i++)
            {
                if (!includeSender)
                {
                    if (connectedUsers[i].userID != senderUserID)
                    {   
                        connectedUsers[i].dataTransferStream.Write(writeBuffer, 0, writeBuffer.Length);
                    }
                }
                else
                    connectedUsers[i].dataTransferStream.Write(writeBuffer, 0, writeBuffer.Length);
            }
        }

        protected internal void NotifyServer(string message, bool userJoined = false, bool userLeft = false)
        {
            if (userJoined)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ResetColor();
            }
            else if (userLeft)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();
            }
            else
                Console.WriteLine(message);
        }

        protected internal void DisconnectUser(int userToDisconnectID)
        {
            var userToDisconnect = connectedUsers.FirstOrDefault(c => c.userID == userToDisconnectID);
            if (userToDisconnect != null)
            {
                connectedUsers.Remove(userToDisconnect);

                string messageAboutLeaving = $"{DateTime.Now.ToShortTimeString()}" +
                    $" : {userToDisconnect.userNickName} left the server.";

                NotifyAllUsers(messageAboutLeaving, userToDisconnectID);
                NotifyServer(messageAboutLeaving, userLeft: true);
            }
        }

        protected internal void DisconnectServer()
        {
            server.Stop();

            for (int i = 0; i < connectedUsers.Count; i++)
                connectedUsers[i].CloseConnection();

            Environment.Exit(0);
        }
    }
}