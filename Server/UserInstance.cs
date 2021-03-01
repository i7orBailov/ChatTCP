using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class UserInstance
    {
        internal int userID { get; private set; }

        protected internal NetworkStream dataTransferStream { get; private set; }

        protected internal string userNickName { get; private set; }

        protected internal string userPassword { get; private set; }

        protected internal bool toRegister { get; private set; }

        static int nextID = 0;
        
        readonly TcpClient user;

        readonly ServerFunctions serverFunction;

        public UserInstance(TcpClient user, ServerFunctions server)
        {
            userID = nextID;
            this.user = user;
            serverFunction = server;
            userNickName = ReceiveMessage();
            userPassword = ReceiveMessage();
            toRegister = bool.Parse(ReceiveMessage());
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

        public void BroadcastMessageToChat()
        {
            while (true)
            {
                string messageToBroadcast = ReceiveMessage();

                if (messageToBroadcast == string.Empty)
                    serverFunction.DisconnectUser(userID);
                else
                {
                    messageToBroadcast = string.Format("{0} : {1} : {2}",
                    DateTime.Now.ToShortTimeString(), userNickName, messageToBroadcast);

                    serverFunction.NotifyServer(messageToBroadcast);
                    serverFunction.NotifyAllUsers(messageToBroadcast, userID, includeSender: true);
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