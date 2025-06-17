using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System;
using System.Windows.Input;
using WpfAppLibrary.Models;

namespace WpfAppLibrary.ViewModel
{
    public class AddBookViewModel : INotifyPropertyChanged
    {
        private readonly LibraryApiService _apiService;
        private string _titleAdd;
        private string _authorAdd;
        private string _genreAdd;
        private string _yearPublishedAdd;
        private bool _isAvailableAdd;

        public ICommand AddBookCommand { get; }
        public ICommand CloseWindowCommand { get; }

        public AddBookViewModel(LibraryApiService apiService)
        {
            _apiService = apiService;

            if (_apiService is null)
                throw new NullReferenceException("apiService is null!");

            AddBookCommand = new RelayCommand(AddBookAsync, CanAddBook);
            CloseWindowCommand = new RelayCommand(CloseWindow);
        }

        public string YearPublishedAdd
        {
            get { return _yearPublishedAdd; }
            set
            {
                _yearPublishedAdd = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public string TitleAdd
        {
            get { return _titleAdd; }
            set
            {
                _titleAdd = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public string AuthorAdd
        {
            get => _authorAdd;
            set
            {
                _authorAdd = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public string GenreAdd
        {
            get => _genreAdd;
            set
            {
                _genreAdd = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public bool IsAvailableAdd
        {
            get => _isAvailableAdd;
            set
            {
                _isAvailableAdd = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        private async void AddBookAsync(object data)
        {
            try
            {
                if (data == null)
                    MessageBox.Show($"Ошибка: Объект равен null!");

                var newBook = new Book
                {
                    Title = TitleAdd,
                    Author = AuthorAdd,
                    Genre = GenreAdd,
                    YearPublished = YearPublishedAdd,
                    IsAvailable = IsAvailableAdd
                };

                var createdBook = await _apiService.CreateModelAsync<Book>(newBook, "api/books");
                newBook.Id = createdBook.Id;

                if (data is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении книги: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddBook(object data)
        {
            return !string.IsNullOrWhiteSpace(TitleAdd) &&
                   !string.IsNullOrWhiteSpace(AuthorAdd) &&
                   !string.IsNullOrWhiteSpace(GenreAdd);
        }

        private void CloseWindow(object data)
        {
            if (data is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        private void UpdateCommandState()
        {
            (AddBookCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
