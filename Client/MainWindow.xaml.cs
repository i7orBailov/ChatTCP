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
        bool isConnected = false;

        public MainWindow()
        {
            InitializeComponent();
            GetDefaultSettings();
        }

        void GetDefaultSettings(bool messageField = true, bool nickName = true)
        {
            if (messageField)
            {
                message.Text = "Enter message";
                message.Foreground = Brushes.Gray;
            }

            if (nickName)
            {
                userNickName.Text = "Enter nick-name";
                userNickName.Foreground = Brushes.Gray;
            }
        }

        MessageBoxResult ShowError(string errorMessage, MessageBoxButton messageBoxButton) =>
            MessageBox.Show(errorMessage, "Error", messageBoxButton, MessageBoxImage.None, MessageBoxResult.None);

        void ConnectDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                if (!string.IsNullOrWhiteSpace(userNickName.Text) && userNickName.Foreground != Brushes.Gray)
                {
                    ConnectToServer();
                    if (client.Connected)
                    {
                        isConnected = true;
                        connectDisconnect.Content = "Disconnect";
                        userNickName.IsEnabled = false;
                    }
                }
                else
                    ShowError("User name can`t be empty!", MessageBoxButton.OK);
            }
            else if (isConnected)
            {
                DisconnectFromServer();
                isConnected = false;
                connectDisconnect.Content = "Connect";
                userNickName.IsEnabled = true;
            }
        }

        void ConnectToServer()
        {
            try
            {
                client.Connect(IPAddress.Loopback, 8080);

                dataTransferStream = client.GetStream();
                SendMessage(userNickName.Text);

                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
            }
            catch (Exception)
            {
                string errorMessage = "Couldn`t connect to the server. Anyway continue?";
                var result = ShowError(errorMessage, MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.No:
                    {
                        Environment.Exit(0);
                        break;
                    }
                    case MessageBoxResult.Yes:
                        break;
                }
            }
        }

        void SendMessage(string inputMessage)
        {
            try
            {
                string messageToSend = inputMessage;
                byte[] writeBuffer = Encoding.UTF8.GetBytes(messageToSend);
                dataTransferStream.Write(writeBuffer, 0, writeBuffer.Length);
            }
            catch (Exception) { ShowError("Connect to the server firstly", MessageBoxButton.OK); }
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
                    completeMessage.Append(Encoding.UTF8.GetString(readBuffer, 0, numberOfBytesRead));
                } while (dataTransferStream.DataAvailable);

                Dispatcher.Invoke(() =>
                {
                    messagesField.Items.Add(completeMessage.ToString());
                    messagesField.ScrollIntoView(messagesField.Items[messagesField.Items.Count - 1]);
                });
            }
        }

        void UserNickName_GotFocus(object sender, RoutedEventArgs e)
        {
            if(userNickName.Foreground == Brushes.Gray)
            {
                userNickName.Text = string.Empty;
                userNickName.Foreground = Brushes.Black;
            }
        }

        void UserNickName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(userNickName.Text))
                GetDefaultSettings(messageField:false);
        }

        void Message_GotFocus(object sender, RoutedEventArgs e)
        {
             if(message.Foreground == Brushes.Gray)
             {
                message.Text = string.Empty;
                message.Foreground = Brushes.Black;
             }
        }

        void Message_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(message.Text))
                GetDefaultSettings(nickName: false);
        }

        void Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (message.Foreground != Brushes.Gray)
                {
                    SendMessage(message.Text);
                    message.Text = string.Empty;
                }
        }

        static void DisconnectFromServer()
        {
            if (dataTransferStream != null)
                dataTransferStream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            => DisconnectFromServer();
    }
}