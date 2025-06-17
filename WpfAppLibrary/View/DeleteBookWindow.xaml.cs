using System.Windows;
using WpfAppLibrary.ViewModel;
using WpfAppLibrary.Models;

namespace WpfAppLibrary.View
{
    /// <summary>
    /// Окно для удаления книги.
    /// </summary>
    public partial class DeleteBookWindow : Window
    {
        public DeleteBookWindow(LibraryApiService apiService, Book bookToDelete)
        {
            InitializeComponent();

            DataContext = new DeleteBookViewModel(apiService, bookToDelete);
        }
    }
}
