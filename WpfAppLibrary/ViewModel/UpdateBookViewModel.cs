using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfAppLibrary.Models;

namespace WpfAppLibrary.ViewModel
{
    public class UpdateBookViewModel : INotifyPropertyChanged
    {
        private readonly LibraryApiService _apiService;
        private readonly Book _bookToUpdate;
        private string _titleUpdate;
        private string _authorUpdate;
        private string _genreUpdate;
        private string _yearPublishedUpdate;
        private bool _isAvailableUpdate;

        public string YearPublishedUpdate
        {
            get { return _yearPublishedUpdate; }
            set
            {
                _yearPublishedUpdate = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public string TitleUpdate
        {
            get { return _titleUpdate; }
            set
            {
                _titleUpdate = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public string AuthorUpdate
        {
            get { return _authorUpdate; }
            set
            {
                _authorUpdate = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public string GenreUpdate
        {
            get { return _genreUpdate; }
            set
            {
                _genreUpdate = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public bool IsAvailableUpdate
        {
            get { return _isAvailableUpdate; }
            set
            {
                _isAvailableUpdate = value;
                OnPropertyChanged();
                UpdateCommandState();
            }
        }

        public ICommand UpdateBookCommand { get; }
        public ICommand CloseWindowCommand { get; }

        public UpdateBookViewModel(LibraryApiService apiService, Book bookToUpdate)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _bookToUpdate = bookToUpdate ?? throw new ArgumentNullException(nameof(bookToUpdate));

            if (_apiService is null)
                throw new NullReferenceException("apiService is null!");

            TitleUpdate = bookToUpdate.Title;
            AuthorUpdate = bookToUpdate.Author;
            GenreUpdate = bookToUpdate.Genre;
            IsAvailableUpdate = bookToUpdate.IsAvailable;
            YearPublishedUpdate = bookToUpdate.YearPublished;

            UpdateBookCommand = new RelayCommand(UpdateBookAsync);
            CloseWindowCommand = new RelayCommand(CloseWindow);
        }

        private async void UpdateBookAsync(object data)
        {
            try
            {
                _bookToUpdate.Title = TitleUpdate;
                _bookToUpdate.Author = AuthorUpdate;
                _bookToUpdate.Genre = GenreUpdate;
                _bookToUpdate.IsAvailable = IsAvailableUpdate;
                _bookToUpdate.YearPublished = YearPublishedUpdate;

                var result = await _apiService.UpdateModelAsync<Book>("api/books", _bookToUpdate, _bookToUpdate.Id);

                if (data is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении книги: {ex.Message}.",
                    "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCommandState()
        {
            (UpdateBookCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
