using System.Windows;

namespace WpfAppLibrary.ViewModel
{
    public interface INavigationService
    {
        void NavigateTo<T>() where T : Window;
        void Close<T>() where T : Window;
    }
}
