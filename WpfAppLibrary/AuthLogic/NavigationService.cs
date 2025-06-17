using System;
using System.Linq;
using System.Windows;

namespace WpfAppLibrary.ViewModel
{
    public class NavigationService : INavigationService
    {
        public void NavigateTo<T>() where T : Window
        {
            var window = Activator.CreateInstance<T>();
            window.Show();
        }

        public void Close<T>() where T : Window
        {
            var window = Application.Current.Windows.OfType<T>().FirstOrDefault();
            window?.Close();
        }
    }
}
