using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{
    public partial class MainWindow : Window
    {
        static TcpClient client;
        static NetworkStream dataTransferStream;
        bool WindowActivatedFirstTime = true;
        string userNickname;
        string userPassword;
        bool userRegister;
        public bool successfullyConnectedToServer { get; private set; }

        public MainWindow(UserAuthorization authorization, string userNick, string password, bool register)
        {   
            InitializeComponent();
            userNickname = userNick;
            userPassword = password;
            userRegister = register;
            textBoxNickname.Text = userNick;
            ConnectToServer();
            if (successfullyConnectedToServer)
                authorization.Close();
            else
                client.Close();
        }

        void ConnectToServer()
        {
            client = new TcpClient();
            client.Connect(IPAddress.Loopback, 8080);
            dataTransferStream = client.GetStream();

            string NickPassRegister = $"name/{userNickname}/" +
                               $"password/{userPassword}/" +
                               $"register/{userRegister}";

            SendMessage(NickPassRegister);
            successfullyConnectedToServer = GetKnownIfUserConnected();
        }

        void SendMessage(string inputMessage)
        {
            try
            {
                string messageToSend = inputMessage;
                byte[] writeBuffer = Encoding.Unicode.GetBytes(messageToSend);
                dataTransferStream.Write(writeBuffer, 0, writeBuffer.Length);
            }
            catch (Exception) { ShowError("Connect to the server firstly", MessageBoxButton.OK); }
        }

        bool GetKnownIfUserConnected()
        {
            byte[] readBuffer = new byte[64];
            StringBuilder completeMessage = new StringBuilder();
            int numberOfBytesRead = 0;
            do
            {
                numberOfBytesRead = dataTransferStream.Read(readBuffer, 0, readBuffer.Length);
                completeMessage.Append(Encoding.Unicode.GetString(readBuffer, 0, numberOfBytesRead));
            } while (dataTransferStream.DataAvailable);

            return bool.Parse(completeMessage.ToString());
        }

        void Window_Activated(object sender, EventArgs e)
        {
            if (WindowActivatedFirstTime)
            {
                GetDefaultSettings();
                WindowActivatedFirstTime = false;
                BroadcastMessagesToChat();
            }
            else
                if (textBoxMessage.IsFocused)
                Message_GotFocus(sender, new RoutedEventArgs());
        }

        void GetDefaultSettings(bool messageField = true)
        {
            if (messageField)
            {
                textBoxMessage.Text = "Enter message";
                textBoxMessage.Foreground = Brushes.Gray;
            }
        }

        void BroadcastMessagesToChat()
        {
            if (successfullyConnectedToServer)
            {
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
            }
        }

        void ReceiveMessage()
        {
            while (true)
            {
                byte[] readBuffer = new byte[64];
                StringBuilder completeMessage = new StringBuilder();
                int numberOfBytesRead = 0;
                do
                {
                    numberOfBytesRead = dataTransferStream.Read(readBuffer, 0, readBuffer.Length);
                    completeMessage.Append(Encoding.Unicode.GetString(readBuffer, 0, numberOfBytesRead));
                } while (dataTransferStream.DataAvailable);

                Dispatcher.Invoke(() =>
                {
                    listBoxMessages.Items.Add(completeMessage.ToString());
                    listBoxMessages.ScrollIntoView(listBoxMessages.Items[listBoxMessages.Items.Count - 1]);
                });
            }
        }

        MessageBoxResult ShowError(string errorMessage, MessageBoxButton messageBoxButton) =>
            MessageBox.Show(errorMessage, "Error", messageBoxButton, MessageBoxImage.None, MessageBoxResult.None);

        void Message_GotFocus(object sender, RoutedEventArgs e)
        {
             if(textBoxMessage.Foreground == Brushes.Gray)
             {
                textBoxMessage.Text = string.Empty;
                textBoxMessage.Foreground = Brushes.Black;
             }
        }

        void Message_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxMessage.Text))
                GetDefaultSettings();
        }

        void Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (textBoxMessage.Foreground != Brushes.Gray)
                {
                    SendMessage(textBoxMessage.Text);
                    textBoxMessage.Text = string.Empty;
                }
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            => DisconnectFromServer();

        static void DisconnectFromServer()
        {
            if (dataTransferStream != null)
                dataTransferStream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }

        void Window_Deactivated(object sender, EventArgs e) =>
            Message_LostFocus(sender, new RoutedEventArgs());
    }
}