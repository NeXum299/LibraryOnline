using System.Windows;
using WpfAppLibrary.ViewModel;

namespace WpfAppLibrary.View
{
    /// <summary>
    /// Окно для добавления новой книги.
    /// </summary>
    public partial class AddBookWindow : Window
    {
        public AddBookWindow(LibraryApiService apiService)
        {
            InitializeComponent();

            DataContext = new AddBookViewModel(apiService);
        }
    }
}
