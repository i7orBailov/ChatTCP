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
                if (UserConnectedToServer(user.userNickName))
                    AnswerToClient(user, false.ToString());
                else
                {
                    if (userAuthorization.successfullyVerified)
                    {
                        ConnectUser(user);
                        var clientThread = new Thread(new ThreadStart(user.BroadcastMessageToChat));
                        clientThread.Start();
                    }
                    AnswerToClient(user, userAuthorization.successfullyVerified.ToString());
                }
                
            }
        }

        protected internal void ConnectUser(UserInstance userToConnect)
        {
            connectedUsers.Add(userToConnect);

            string messageAboutJoining = $"{DateTime.Now.ToShortTimeString()}" +
                    $" : {userToConnect.userNickName} connected to the server.";

            // TODO : clause for server : user 'logged'/'registered' and then connected to the server

            NotifyAllUsers(messageAboutJoining, userToConnect.userID);
            NotifyServer(messageAboutJoining, ConsoleColor.Green);
        }

        void AnswerToClient(UserInstance userToWhomAnswer, string messageToAnswer)
        {
            try
            {
                byte[] writeBuffer = Encoding.Unicode.GetBytes(messageToAnswer);
                userToWhomAnswer.dataTransferStream.Write(writeBuffer, 0, writeBuffer.Length);
            }
            catch (Exception) { Console.WriteLine("Could not answer to client."); }
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

        protected internal void NotifyServer(string message, ConsoleColor consoleColor = ConsoleColor.Blue)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
            Console.ResetColor();
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
                NotifyServer(messageAboutLeaving, ConsoleColor.Red);
            }
        }

        protected internal void DisconnectServer()
        {
            server.Stop();

            for (int i = 0; i < connectedUsers.Count; i++)
                connectedUsers[i].CloseConnection();

            Environment.Exit(0);
        }

        protected internal void ShowInfo()
        {
            while (true)
            {
                var requestCommands = new string[]
                { "--help", "--online", "--kick" };

                string requestedCommand = Console.ReadLine();
                if (requestedCommand == requestCommands[0])
                    ShowHelpInfo(requestCommands);
                else if (requestedCommand == requestCommands[1])
                    ShowOnlineStatus();
                else if (requestedCommand == requestCommands[2])
                    KickUser();
                else
                    ShowError();
            }
        }

        void ShowHelpInfo(params string[] availableCommands)
        {
            NotifyServer("commands available:", ConsoleColor.Gray);
            foreach (var command in availableCommands)
            {
                NotifyServer($"\t{command}", ConsoleColor.Gray);
            }
        }

        void ShowOnlineStatus()
        {
            if (connectedUsers.Count == 0)
                NotifyServer("No users online", ConsoleColor.Gray);
            else
            {
                NotifyServer($"Users online: {connectedUsers.Count}", ConsoleColor.Gray);
                foreach (var user in connectedUsers)
                {
                    NotifyServer($"\t[ID] {user.userID} : {user.userNickName}", ConsoleColor.Gray);
                }
            }
        }

        void KickUser()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("ID of user to kick: ");
            bool validID = int.TryParse(Console.ReadLine(), out int userToKickID);
            if (validID)
            {
                var userToKick = connectedUsers.FirstOrDefault(c => c.userID == userToKickID);
                if (userToKick != null)
                {
                    string messageAboutKicking = $"{DateTime.Now.ToShortTimeString()}" +
                        $" : {userToKick.userNickName} was kicked from the server.";

                    NotifyServer(messageAboutKicking, ConsoleColor.Red);
                    NotifyAllUsers(messageAboutKicking, userToKickID, includeSender: true);

                    connectedUsers.Remove(userToKick);
                }
                else
                    NotifyServer("No such a user online", ConsoleColor.DarkRed);
            }
            else
                NotifyServer("Only digit allowed", ConsoleColor.DarkRed);
            Console.ResetColor();

        }

        public bool UserConnectedToServer(int ID)
        {
            var userToCheck = connectedUsers.FirstOrDefault(c => c.userID == ID);
            return userToCheck == null ? false : true;
        }

        public bool UserConnectedToServer(string Nickname)
        {
            var userToCheck = connectedUsers.FirstOrDefault(c => c.userNickName == Nickname);
            return userToCheck == null ? false : true;
        }

        void ShowError() =>
            NotifyServer("Unknown command. --help for more details", ConsoleColor.DarkRed);
    }
}