using System.Windows;
using System.Windows.Controls; // Для PasswordBox
using Ynost.ViewModels;

namespace Ynost.View
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            // Если DataContext создается в XAML, то можно получить к нему доступ так:
            var viewModel = DataContext as LoginViewModel;
            if (viewModel != null)
            {
                viewModel.CloseAction = () =>
                {
                    // Устанавливаем DialogResult в зависимости от успеха логина
                    // DialogResult можно проверить в вызывающем коде
                    this.DialogResult = viewModel.LoginSuccessful;
                    this.Close();
                };
            }
        }

        // Временное решение для передачи пароля из PasswordBox в ViewModel как string
        // В идеале, ViewModel должен работать с SecureString, или использовать Attached Property/Behavior
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }

        // Публичное свойство для доступа к результату логина извне, если нужно
        public LoginResultRole AuthenticatedRole => (DataContext as LoginViewModel)?.AuthenticatedUserRole ?? LoginResultRole.None;
        public bool? WasLoginSuccessful => this.DialogResult;

    }
}