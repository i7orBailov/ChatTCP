using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    public partial class UserAuthorization : Window
    {
        bool passwordIsValid;

        bool usernickIsValid;

        public UserAuthorization()
        {
            InitializeComponent();
            ComboBoxRegisterLogin.SelectedIndex = 0;
        }

        void ButtonRegisterOrLogin_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxPassword.Text.Count() < HiddenPassword.Password.Count())
                textBoxPassword.Text = HiddenPassword.Password;
            ValidatePassword();
            ValidateUsername();
            if (passwordIsValid && usernickIsValid)
            {
                if (ComboBoxRegisterLogin.SelectedIndex == 0)
                    LoginOrRegisterUser(register: true);
                else
                    LoginOrRegisterUser(register: false);
            }
        }

        void ValidateUsername()
        {
            if (string.IsNullOrWhiteSpace(textBoxNickname.Text))
                NotifyError("Username can not be empty");
            else if (textBoxNickname.Text.Any(x => char.IsWhiteSpace(x)))
                NotifyError("Username should not contain empty spaces");
            else if (textBoxNickname.Text.Any(x => x == '/'))
                NotifyError("Username can not contain '/' symbol");
            else
                usernickIsValid = true;
        }

        void ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(textBoxPassword.Text))
                NotifyError("Password can not be empty");
            else if (textBoxPassword.Text.Any(x => char.IsWhiteSpace(x)))
                NotifyError("Password should not contain empty spaces");
            else if (textBoxPassword.Text.Any(x => x == '/'))
                NotifyError("Password can not contain '/' symbol");
            else if (textBoxPassword.Text.Where(x => char.IsLetter(x)).Count() == 0)
                NotifyError("Password should contain at least one letter");
            else
                passwordIsValid = true;
        }

        void LoginOrRegisterUser(bool register)
        {   
            try
            {
                string name = textBoxNickname.Text;
                string password = textBoxPassword.Text;
                var chatApplication = new MainWindow(this, name, password, register);
                if (chatApplication.successfullyConnectedToServer)
                {
                    chatApplication.Show();
                }
                else
                {
                    if (register)
                        NotifyError("Seems, such a user is already registered or being online");
                    else
                        NotifyError("Wrong password or user is not registered yet or being online");
                }
            }
            catch (Exception) { NotifyError("Could not connect to the server"); }
        }

        void NotifyError(string errorMessage) =>
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.None);

        private void RegisterLogin_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ButtonRegisterLogin.Content = ComboBoxRegisterLogin.SelectedIndex == 0 ?
                "Register" : "Login";

        private void CheckBoxShowPass_Checked(object sender, RoutedEventArgs e)
        {
            textBoxPassword.Visibility = Visibility.Visible;
            HiddenPassword.Visibility = Visibility.Hidden;
            textBoxPassword.Text = HiddenPassword.Password;
        }

        private void CheckBoxShowPass_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxPassword.Visibility = Visibility.Hidden;
            HiddenPassword.Visibility = Visibility.Visible;
            HiddenPassword.Password = textBoxPassword.Text;
        }
    }
}