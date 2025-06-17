using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfAppLibrary.Models;
using WpfAppLibrary.View;

namespace WpfAppLibrary.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; }

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;

            LoginCommand = new RelayCommand(async _ => await Login(), _ => CanLogin());
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
        }

        private bool CanLogin() => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

        private async Task Login()
        {
            try
            {
                var user = new LoginModel { Username = Username, Password = Password };
                bool isAuthenticated = await _authService.Authenticate(user);

                if (isAuthenticated)
                {
                    _navigationService.NavigateTo<MainWindow>();
                    _navigationService.Close<LoginWindow>();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
