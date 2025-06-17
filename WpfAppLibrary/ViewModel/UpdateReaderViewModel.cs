using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfAppLibrary.Models;

namespace WpfAppLibrary.ViewModel
{
    public class UpdateReaderViewModel : INotifyPropertyChanged
    {
        private readonly LibraryApiService _apiService;
        private readonly Reader _readerToUpdate;
        private string _nameUpdate;
        private string _emailUpdate;

        public string NameUpdate
        {
            get => _nameUpdate;
            set
            {
                _nameUpdate = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public string EmailUpdate
        {
            get => _emailUpdate;
            set
            {
                _emailUpdate = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public ICommand UpdateReaderCommand { get; }
        public ICommand CloseWindowCommand { get; }

        public UpdateReaderViewModel(LibraryApiService apiService, Reader readerToUpdate)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _readerToUpdate = readerToUpdate ?? throw new ArgumentNullException(nameof(readerToUpdate));

            if (_apiService is null)
                throw new NullReferenceException("apiService is null!");

            NameUpdate = readerToUpdate.Name;
            EmailUpdate = readerToUpdate.Email;

            UpdateReaderCommand = new RelayCommand(UpdateReaderAsync);
            CloseWindowCommand = new RelayCommand(CloseWindow);
        }

        private async void UpdateReaderAsync(object data)
        {
            try
            {
                _readerToUpdate.Name = NameUpdate;
                _readerToUpdate.Email = EmailUpdate;

                var result = await _apiService.UpdateModelAsync<Reader>("api/readers", _readerToUpdate, _readerToUpdate.Id);

                if (data is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении читателя: {ex.Message}.",
                    "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCommandState()
        {
            (UpdateReaderCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void CloseWindow(object data)
        {
            if (data is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
