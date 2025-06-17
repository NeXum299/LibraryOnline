using System.Windows;
using WpfAppLibrary.Models;
using WpfAppLibrary.ViewModel;

namespace WpfAppLibrary.View
{
    /// <summary>
    /// Окно для измениния (обновления данных) книги.
    /// </summary>
    public partial class UpdateBookWindow : Window
    {
        public UpdateBookWindow(LibraryApiService apiService, Book bookToUpdate)
        {
            InitializeComponent();

            DataContext = new UpdateBookViewModel(apiService, bookToUpdate);
        }
    }
}
