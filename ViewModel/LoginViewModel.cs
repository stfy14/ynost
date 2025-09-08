using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// Добавь using для настроек
using Ynost.Properties; // Замени Ynost на имя твоего корневого неймспейса, если оно другое
namespace Ynost.ViewModels
{
    public enum LoginResultRole { None, Editor, Viewer }
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _username;

        [ObservableProperty]
        private string? _password;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _rememberMe;

        public LoginResultRole AuthenticatedUserRole { get; private set; } = LoginResultRole.None;
        public bool LoginSuccessful { get; private set; } = false;

        public IRelayCommand LoginCommand { get; }
        public Action? CloseAction { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(AttemptLogin);
            if (Settings.Default.RememberLastUser)
            {
                Username = Settings.Default.LastUsername;
                // Password = Settings.Default.LastPassword; 
                RememberMe = true; 
            }
        }

        private void AttemptLogin()
        {
            ErrorMessage = null;
            LoginSuccessful = false;
            AuthenticatedUserRole = LoginResultRole.None;

            if (Username == "admin" && Password == "admin")
            {
                AuthenticatedUserRole = LoginResultRole.Editor;
                LoginSuccessful = true;
            }
            else if (Username == "view" && Password == "view")
            {
                AuthenticatedUserRole = LoginResultRole.Viewer;
                LoginSuccessful = true;
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль.";
                return;
            }

            if (LoginSuccessful)
            {
                if (RememberMe)
                {
                    Settings.Default.LastUsername = Username;
                    Settings.Default.LastPassword = Password; 
                    Settings.Default.RememberLastUser = true;
                }
                else
                {
                    Settings.Default.LastUsername = string.Empty;
                    Settings.Default.LastPassword = string.Empty;
                    Settings.Default.RememberLastUser = false;
                }
                Settings.Default.Save(); 
            }

            CloseAction?.Invoke();
        }
    }
}