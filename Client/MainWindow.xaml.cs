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
        static TcpClient client = new TcpClient();
        static NetworkStream dataTransferStream;
        //bool isConnectedToServer = false;
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
        }

        void GetDefaultSettings(bool messageField = true, bool nickName = true)
        {
            if (messageField)
            {
                textBoxMessage.Text = "Enter message";
                textBoxMessage.Foreground = Brushes.Gray;
            }

            if (nickName)
            {
                textBoxNickname.Text = "Enter nick-name";
                textBoxNickname.Foreground = Brushes.Gray;
            }
        }

        MessageBoxResult ShowError(string errorMessage, MessageBoxButton messageBoxButton) =>
            MessageBox.Show(errorMessage, "Error", messageBoxButton, MessageBoxImage.None, MessageBoxResult.None);

        //void ConnectDisconnect_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!isConnectedToServer)
        //    {
        //        if (!string.IsNullOrWhiteSpace(userNickName.Text) && userNickName.Foreground != Brushes.Gray)
        //        {
        //            ConnectToServer();
        //            if (client.Connected)
        //            {
        //                isConnectedToServer = true;
        //                connectDisconnect.Content = "Disconnect";
        //                userNickName.IsEnabled = false;
        //            }
        //        }
        //        else
        //            ShowError("User name can`t be empty!", MessageBoxButton.OK);
        //    }
        //    else if (isConnectedToServer)
        //    {
        //        DisconnectFromServer();
        //        isConnectedToServer = false;
        //        connectDisconnect.Content = "Connect";
        //        userNickName.IsEnabled = true;
        //    }
        //}

        void ConnectToServer()
        {
            try
            {
                client.Connect(IPAddress.Loopback, 8080);
                dataTransferStream = client.GetStream();

                string forServer = $"name/{userNickname}/" +
                                   $"password/{userPassword}/" +
                                   $"register/{userRegister}";

                SendMessage(forServer);
                successfullyConnectedToServer = GetKnownIfUserConnected();
            }

            catch (Exception ex)
            {
                //    //string errorMessage = "Couldn`t connect to the server. Anyway continue?";
                //    //var result = ShowError(errorMessage, MessageBoxButton.YesNo);
                //    //switch (result)
                //    //{
                //    //    case MessageBoxResult.No:
                //    //    {
                //    //        Environment.Exit(0);
                //    //        break;
                //    //    }
                //    //    case MessageBoxResult.Yes:
                //    //        break;
                //    //}
                throw ex;
            }
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

        void BroadcastMessagesToChat()
        {
            if (successfullyConnectedToServer)
            {
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
            }
        }

        void UserNickName_GotFocus(object sender, RoutedEventArgs e)
        {
            if(textBoxNickname.Foreground == Brushes.Gray)
            {
                textBoxNickname.Text = string.Empty;
                textBoxNickname.Foreground = Brushes.Black;
            }
        }

        void UserNickName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxNickname.Text))
                GetDefaultSettings(messageField:false);
        }

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
                GetDefaultSettings(nickName: false);
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

        void Window_Deactivated(object sender, EventArgs e)
        {
            Message_LostFocus(sender, new RoutedEventArgs());
            UserNickName_LostFocus(sender, new RoutedEventArgs());
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
                else if (textBoxNickname.IsFocused)
                    UserNickName_GotFocus(sender, new RoutedEventArgs());
        }
    }
}