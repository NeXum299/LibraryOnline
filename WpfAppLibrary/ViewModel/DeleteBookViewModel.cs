using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfAppLibrary.Models;

namespace WpfAppLibrary.ViewModel
{
    public class DeleteBookViewModel
    {
        private readonly LibraryApiService _apiService;
        private readonly Book _bookToDelete;

        public ICommand YesCommand { get; }
        public ICommand NoCommand { get; }

        public DeleteBookViewModel(LibraryApiService apiService, Book bookToDelete)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _bookToDelete = bookToDelete ?? throw new ArgumentNullException(nameof(bookToDelete));

            if (_apiService is null)
                throw new NullReferenceException("apiService is null!");

            YesCommand = new RelayCommand(ConfirmDelete);
            NoCommand = new RelayCommand(CloseWindow);
        }

        private async void ConfirmDelete(object data)
        {
            try
            {
                await _apiService.DeleteModelAsync("api/books", _bookToDelete.Id);

                if (data is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении книги: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow(object data)
        {
            if (data is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}
