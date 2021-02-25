using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ClientInstance
    {
        protected internal int userID { get; private set; }

        protected internal NetworkStream dataTransferStream { get; private set; }

        protected internal string userNickName { get; set; }

        static int nextID = 0;

        readonly TcpClient user;

        readonly ServerFunctions server;

        public ClientInstance(TcpClient user, ServerFunctions server)
        {
            this.user = user;
            this.server = server;
            userID = nextID;
            this.server.ConnectUser(this);
            nextID++;
        }

        protected internal string ReceiveMessage()
        {
            dataTransferStream = user.GetStream();

            byte[] readBuffer = new byte[64];
            StringBuilder completeMessage = new StringBuilder();
            int numberOfBytesRead = 0;
            do
            {
                numberOfBytesRead = dataTransferStream.Read(readBuffer, 0, readBuffer.Length);
                completeMessage.Append(Encoding.Unicode.GetString(readBuffer, 0, numberOfBytesRead));
            } while (dataTransferStream.DataAvailable);

            return completeMessage.ToString();
        }

        public void BroadcastChat()
        {
            while (true)
            {
                string messageToBroadcast = ReceiveMessage();

                if (messageToBroadcast == string.Empty)
                    server.DisconnectUser(userID);
                else
                {
                    messageToBroadcast = string.Format("{0} : {1} : {2}",
                    DateTime.Now.ToShortTimeString(), userNickName, messageToBroadcast);

                    server.NotifyServer(messageToBroadcast);
                    server.NotifyAllUsers(messageToBroadcast, userID, includeSender: true);
                }
            }
        }

        protected internal void CloseConnection()
        {
            if (dataTransferStream != null)
                dataTransferStream.Close();
            if (user != null)
                user.Close();
        }
    }
}