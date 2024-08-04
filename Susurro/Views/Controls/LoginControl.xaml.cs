using System.Windows;
using System.Windows.Controls;

namespace Susurro.Views.Controls
{
    public partial class LoginControl : UserControl
    {
        public LoginControl()
        {
            InitializeComponent();
        }

        private void LoginPwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((dynamic)this.DataContext).LoginPassword = ((PasswordBox)sender).Password;
        }

        private void CreatePwdBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((dynamic)this.DataContext).CreatePassword1 = ((PasswordBox)sender).Password;
        }

        private void CreatePwdBox2_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((dynamic)this.DataContext).CreatePassword2 = ((PasswordBox)sender).Password;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ClearPasswords();
        }

        private void ClearPasswords()
        {
            ((dynamic)this.DataContext).PasswordsLocked = true;
            LoginPwdBox.Password = null;
            CreatePwdBox1.Password = null;
            CreatePwdBox2.Password = null;
            ((dynamic)this.DataContext).PasswordsLocked = false;
        }
        private void CreateUserButton_Click(object sender, RoutedEventArgs e)
        {
            ClearPasswords();
        }
    }
}
