using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    public partial class UserAuthorization : Window
    {
        public UserAuthorization()
        {
            InitializeComponent();
        }
        // '/' symbol - forbidden in usage of nick/pass
        void ButtonRegisterOrLogin_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonRegisterLogin.SelectedIndex == 0)
                LoginOrRegisterUser(register: true);
            else
                LoginOrRegisterUser(register: false);
        }

        void LoginOrRegisterUser(bool register)
        {
            string name = textBoxNickname.Text;
            string password = textBoxPassword.Text;
            var chatApplication = new MainWindow(this, name, password, register);
            if (chatApplication.successfullyConnectedToServer)
            {
                chatApplication.Show();
            }
        }

        private void RegisterLogin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO : check error with compilation
            //Register.Content = registerLogin.SelectedIndex == 0 ? "Register" : "Login";
        }


    }
}
