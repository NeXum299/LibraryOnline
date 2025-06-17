using System.Windows;
using WpfAppLibrary.Models;
using WpfAppLibrary.ViewModel;

namespace WpfAppLibrary.View
{
    /// <summary>
    /// Окно для изменения (обновления данных) читателя.
    /// </summary>
    public partial class UpdateReaderWindow : Window
    {
        public UpdateReaderWindow(LibraryApiService apiService, Reader readerToUpdate)
        {
            InitializeComponent();

            DataContext = new UpdateReaderViewModel(apiService, readerToUpdate);
        }
    }
}
