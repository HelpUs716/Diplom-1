using System.Linq;
using System.Windows;
using Учет.Core;
using Учет.Enums;
using Учет.Services;
using UserEntity = Учет.Models.User;

namespace Учет.ViewModels
{
    public class AuthViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly IUserSessionService _session;

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); RefreshCommands(); }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); RefreshCommands(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public RelayCommand LoginCommand { get; }

        public AuthViewModel(IUserService userService, IUserSessionService session)
        {
            _userService = userService;
            _session = session;
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        private void RefreshCommands() => LoginCommand.RaiseCanExecuteChanged();

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var userEntity = await _userService.AuthenticateAsync(Login, Password);
                if (userEntity != null)
                {
                    _session.SetSession((UserRole)userEntity.Role, userEntity.Login);

                    if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) is Window win)
                        win.DialogResult = true;
                }
                else
                {
                    ErrorMessage = "Неверный логин или пароль";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка подключения к БД: {ex.Message}";
            }
        }
    }
}