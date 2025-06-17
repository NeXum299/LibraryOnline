using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;
using System.Net.Http;
using System.Windows;
using WpfAppLibrary.View;
using WpfAppLibrary.ViewModel;

namespace WpfAppLibrary
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainWindow>();

            services.AddTransient<LoginWindow>(provider => new LoginWindow(
                provider.GetRequiredService<LoginViewModel>()));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.DataContext = _serviceProvider.GetRequiredService<LoginViewModel>();
            loginWindow.Show();

            base.OnStartup(e);
        }
    }
}
