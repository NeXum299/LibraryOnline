using System.Windows;
using WpfAppLibrary.ViewModel;

namespace WpfAppLibrary.View
{
    /// <summary>
    /// Окно для регистрации нового читателя.
    /// </summary>
    public partial class AddReaderWindow : Window
    {
        public AddReaderWindow(LibraryApiService apiService)
        {
            InitializeComponent();

            DataContext = new AddReaderViewModel(apiService);
        }
    }
}
