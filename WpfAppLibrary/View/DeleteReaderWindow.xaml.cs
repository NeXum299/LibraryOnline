using System.Windows;
using WpfAppLibrary.Models;
using WpfAppLibrary.ViewModel;

namespace WpfAppLibrary.View
{
    /// <summary>
    /// Окно для удаления читателя.
    /// </summary>
    public partial class DeleteReaderWindow : Window
    {
        public DeleteReaderWindow(LibraryApiService libraryApiService, Reader readerToDelete)
        {
            InitializeComponent();

            DataContext = new DeleteReaderViewModel(libraryApiService, readerToDelete);
        }
    }
}
