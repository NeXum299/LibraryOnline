using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfAppLibrary.Models;

namespace WpfAppLibrary.ViewModel
{
    public class DeleteReaderViewModel
    {
        private readonly LibraryApiService _apiService;
        private readonly Reader _readerToDelete;

        public ICommand YesCommand { get; }
        public ICommand NoCommand { get; }

        public DeleteReaderViewModel(LibraryApiService apiService, Reader readerToDelete)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _readerToDelete = readerToDelete ?? throw new ArgumentNullException(nameof(readerToDelete));

            if (_apiService is null)
                throw new NullReferenceException("apiService is null!");

            YesCommand = new RelayCommand(ConfirmDelete);
            NoCommand = new RelayCommand(CloseWindow);
        }

        private async void ConfirmDelete(object data)
        {
            try
            {
                await _apiService.DeleteModelAsync("api/readers", _readerToDelete.Id);

                if (data is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении читателя: {ex.Message}.",
                    "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
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
