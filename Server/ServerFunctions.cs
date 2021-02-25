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
        List<ClientInstance> users = new List<ClientInstance>();

        protected internal void ListenConnections()
        {
            server = new TcpListener(IPAddress.Any, 8080);
            server.Start();

            Console.WriteLine("Server is active. Waiting for connections...");

            while (true)
            {
                TcpClient tcpClient = server.AcceptTcpClient();

                ClientInstance user = new ClientInstance(tcpClient, this);
                Thread clientThread = new Thread(new ThreadStart(user.BroadcastChat));
                clientThread.Start();
            }
        }

        protected internal void ConnectUser(ClientInstance userToAdd)
        {
            users.Add(userToAdd);
            userToAdd.userNickName = userToAdd.ReceiveMessage();

            string messageAboutJoining = $"{DateTime.Now.ToShortTimeString()}" +
                    $" : {userToAdd.userNickName} connected to the server.";
            
            NotifyAllUsers(messageAboutJoining, userToAdd.userID);
            NotifyServer(messageAboutJoining, userJoined: true);
        }
        
        protected internal void NotifyAllUsers(string messageToSend, int senderUserID, bool includeSender = false)
        {
            byte[] writeBuffer = Encoding.Unicode.GetBytes(messageToSend);

            for (int i = 0; i < users.Count; i++)
            {
                if (!includeSender)
                {
                    if (users[i].userID != senderUserID)
                    {   
                        users[i].dataTransferStream.Write(writeBuffer, 0, writeBuffer.Length);
                    }
                }
                else
                    users[i].dataTransferStream.Write(writeBuffer, 0, writeBuffer.Length);
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

        protected internal void DisconnectUser(int userID)
        {
            var userToRemove = users.FirstOrDefault(c => c.userID == userID);
            if (userToRemove != null)
            {
                users.Remove(userToRemove);

                string messageAboutLeaving = $"{DateTime.Now.ToShortTimeString()}" +
                    $" : {userToRemove.userNickName} left the server.";

                NotifyAllUsers(messageAboutLeaving, userID);
                NotifyServer(messageAboutLeaving, userLeft: true);
            }
        }

        protected internal void DisconnectServer()
        {
            server.Stop();

            for (int i = 0; i < users.Count; i++)
                users[i].CloseConnection();

            Environment.Exit(0);
        }
    }
}