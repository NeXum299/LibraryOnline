using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfAppLibrary.Models;

namespace WpfAppLibrary.ViewModel
{
    public class AddReaderViewModel : INotifyPropertyChanged
    {
        private readonly LibraryApiService _apiService;
        private string _name;
        private string _email;

        public ICommand AddReaderCommand { get; }
        public ICommand CloseWindowCommand { get; }

        public string NameAdd
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string EmailAdd
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public AddReaderViewModel(LibraryApiService apiService)
        {
            _apiService = apiService;

            if (_apiService is null)
                throw new NullReferenceException("apiService is null!");

            AddReaderCommand = new RelayCommand(AddReaderAsync, CanReaderBook);
            CloseWindowCommand = new RelayCommand(CloseWindow);
        }

        private bool CanReaderBook(object data) =>
            !string.IsNullOrWhiteSpace(NameAdd) &&
            !string.IsNullOrWhiteSpace(EmailAdd);


        private async void AddReaderAsync(object data)
        {
            try
            {
                if (data == null)
                    MessageBox.Show($"Ошибка: Объект равен null!");

                var newReader = new Reader
                {
                    Name = NameAdd,
                    Email = EmailAdd
                };

                var createdReader = await _apiService.CreateModelAsync<Reader>(newReader, "api/readers");
                newReader.Id = createdReader.Id;

                if (data is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении читателя: {ex.Message}.",
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
