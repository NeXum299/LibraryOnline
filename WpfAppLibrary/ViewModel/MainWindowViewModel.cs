using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfAppLibrary.Models;
using WpfAppLibrary.View;

namespace WpfAppLibrary.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly LibraryApiService _apiService;

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                ResetFilters();
            }
        }

        // Свойства книги
        private string _titleFilter;
        public string TitleFilter
        {
            get => _titleFilter;
            set
            {
                _titleFilter = value;
                OnPropertyChanged();
                FilterBooks();
            }
        }

        private string _authorFilter;
        public string AuthorFilter
        {
            get => _authorFilter;
            set
            {
                _authorFilter = value;
                OnPropertyChanged();
                FilterBooks();
            }
        }

        private string _genreFilter;
        public string GenreFilter
        {
            get => _genreFilter;
            set
            {
                _genreFilter = value;
                OnPropertyChanged();
                FilterBooks();
            }
        }

        private string _isAvailableFilter;
        public string IsAvailableFilter
        {
            get => _isAvailableFilter;
            set
            {
                _isAvailableFilter = value;
                OnPropertyChanged();
                FilterBooks();
            }
        }

        private ObservableCollection<Book> _allBooks;
        private ObservableCollection<Book> _filteredBooks;
        public ObservableCollection<Book> Books
        {
            get => _filteredBooks;
            set
            {
                _filteredBooks = value;
                OnPropertyChanged();
            }
        }

        private Book _selectedBook;
        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged();
            }
        }

        // Свойства читателя
        private string _nameFilter;
        private string _emailFilter;

        public string NameFilter
        {
            get => _nameFilter;
            set
            {
                _nameFilter = value;
                OnPropertyChanged();
                FilterReaders();
            }
        }

        public string EmailFilter
        {
            get => _emailFilter;
            set
            {
                _emailFilter = value;
                OnPropertyChanged();
                FilterReaders();
            }
        }

        private ObservableCollection<Reader> _allReaders;
        private ObservableCollection<Reader> _filteredReaders;
        public ObservableCollection<Reader> Readers
        {
            get => _filteredReaders;
            set
            {
                _filteredReaders = value;
                OnPropertyChanged();
            }
        }

        private Reader _selectedReader;
        public Reader SelectedReader
        {
            get => _selectedReader;
            set
            {
                _selectedReader = value;
                OnPropertyChanged();
            }
        }

        // Свойства записи
        private int _bookIdFilter;
        private int _readerIdFilter;
        private DateTime _borrowDateFilter;
        private DateTime _returnDateFilter;

        public int BookIdFilter
        {
            get => _bookIdFilter;
            set
            {
                _bookIdFilter = value;
                OnPropertyChanged();
                FilterRecords();
            }
        }

        public int ReaderIdFilter
        {
            get => _readerIdFilter;
            set
            {
                _readerIdFilter = value;
                OnPropertyChanged();
                FilterRecords();
            }
        }

        public DateTime BorrowDateFilter
        {
            get => _borrowDateFilter;
            set
            {
                _borrowDateFilter = value;
                OnPropertyChanged();
                FilterRecords();
            }
        }

        public DateTime ReturnDateFilter
        {
            get => _returnDateFilter;
            set
            {
                _returnDateFilter = value;
                OnPropertyChanged();
                FilterRecords();
            }
        }

        private ObservableCollection<BorrowRecord> _allRecords;
        private ObservableCollection<BorrowRecord> _filteredRecords;
        public ObservableCollection<BorrowRecord> Records
        {
            get => _filteredRecords;
            set
            {
                _filteredRecords = value;
                OnPropertyChanged();
            }
        }

        private BorrowRecord _selectedRecord;
        public BorrowRecord SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                _selectedRecord = value;
                OnPropertyChanged();
            }
        }

        // Команды
        public ICommand OpenAddBookWindowCommand { get; }
        public ICommand GetListBooksCommand { get; }
        public ICommand GetListReadersCommand { get; }
        public ICommand GetListRecordsCommand { get; }
        public ICommand OpenDeleteBookWindowCommand { get; }
        public ICommand OpenUpdateBookWindowCommand { get; }
        public ICommand ResetFiltersBookCommand { get; }
        public ICommand ResetFiltersReaderCommand { get; }
        public ICommand ResetFiltersRecordCommand { get; }
        public ICommand OpenAddReaderWindowCommand { get; }
        public ICommand OpenUpdateReaderWindowCommand { get; }
        public ICommand OpenDeleteReaderWindowCommand { get; }
        public ICommand GiveBookReaderCommand { get; }
        public ICommand MarkToReturnCommand { get; }

        public MainWindowViewModel(LibraryApiService apiService)
        {
            _apiService = apiService;
            OpenAddBookWindowCommand = new RelayCommand(OpenAddBookWindow);
            OpenAddReaderWindowCommand = new RelayCommand(OpenAddReaderWindow);
            GetListBooksCommand = new RelayCommand(async _ => await GetListBooksAsync());
            GetListReadersCommand = new RelayCommand(async _ => await GetListReadersAsync());
            GetListRecordsCommand = new RelayCommand(async _ => await GetListRecordsAsync());
            OpenDeleteBookWindowCommand = new RelayCommand(OpenDeleteBookWindow);
            OpenDeleteReaderWindowCommand = new RelayCommand(OpenDeleteReaderWindow);
            OpenUpdateBookWindowCommand = new RelayCommand(OpenUpdateBookWindow);
            OpenUpdateReaderWindowCommand = new RelayCommand(OpenUpdateReaderWindow);
            GiveBookReaderCommand = new RelayCommand(GiveBookReaderAsync);
            MarkToReturnCommand = new RelayCommand(MarkToReturnBook);

            Books = new ObservableCollection<Book>();
            Readers = new ObservableCollection<Reader>();
            Records = new ObservableCollection<BorrowRecord>();

            GetListBooksAsync().ConfigureAwait(false);
            GetListReadersAsync().ConfigureAwait(false);
            GetListRecordsAsync().ConfigureAwait(false);

            ResetFiltersBookCommand = new RelayCommand(_ => ResetBookFilters());
            ResetFiltersReaderCommand = new RelayCommand(_ => ResetReaderFilters());
            ResetFiltersRecordCommand = new RelayCommand(_ => ResetRecordFilters());
        }

        private void ResetBookFilters()
        {
            TitleFilter = string.Empty;
            AuthorFilter = string.Empty;
            GenreFilter = string.Empty;
            IsAvailableFilter = string.Empty;
        }

        private void ResetReaderFilters()
        {
            NameFilter = string.Empty;
            EmailFilter = string.Empty;
        }

        private void ResetRecordFilters()
        {
            BookIdFilter = 0;
            ReaderIdFilter = 0;
            BorrowDateFilter = default;
            ReturnDateFilter = default;
        }

        private async void OpenUpdateBookWindow(object parameter)
        {
            try
            {
                if (_selectedBook == null)
                {
                    MessageBox.Show("Выберите книгу перед изменением!", "Ошибка!",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updateBookWindow = new UpdateBookWindow(_apiService, _selectedBook);

                if (updateBookWindow.ShowDialog() == true)
                {
                    MessageBox.Show("Книга успешно обновлена!", "Успех!",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    await GetListBooksAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении книги! Ошибка: {ex.Message}.");
                MessageBox.Show($"Ошибка при обновлении книги!");
                throw;
            }
        }

        private async void GiveBookReaderAsync(object data)
        {
            try
            {
                if (_selectedBook == null)
                {
                    MessageBox.Show("Выберите книгу перед добавлением записи!", "Ошибка!",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedReader == null)
                {
                    MessageBox.Show("Выберите читателя перед добавлением записи!", "Ошибка!",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newRecord = new BorrowRecord
                {
                    BookId = _selectedBook.Id,
                    BorrowDate = DateTime.Now,
                    IsReturned = false,
                    ReaderId = _selectedReader.Id,
                    ReturnDate = DateTime.MinValue
                };

                var createdRecord = await _apiService.CreateModelAsync<BorrowRecord>(newRecord, "api/borrow_records");
                newRecord.Id = createdRecord.Id;


                MessageBox.Show("Запись успешно добавлена!", "Успех!",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                await GetListRecordsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении записи: {ex.Message}.",
                    "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private async void MarkToReturnBook(object data)
        {
            try
            {
                if (_selectedRecord == null)
                {
                    MessageBox.Show("Выберите запись перед удалением!", "Ошибка!",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _apiService.DeleteModelAsync("api/borrow_records", _selectedRecord.Id);

                MessageBox.Show("Запись успешно удалена!", "Успех!",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                await GetListRecordsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении записи: {ex.Message}.",
                    "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private async void OpenUpdateReaderWindow(object parameter)
        {
            try
            {
                if (_selectedReader == null)
                {
                    MessageBox.Show("Выберите читателя перед изменением!", "Ошибка!",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updateReaderWindow = new UpdateReaderWindow(_apiService, _selectedReader);

                if (updateReaderWindow.ShowDialog() == true)
                {
                    MessageBox.Show("Читатель успешно обновлён!", "Успех!",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    await GetListReadersAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении читателя! Ошибка: {ex.Message}.");
                MessageBox.Show($"Ошибка при обновлении читателя!");
                throw;
            }
        }
        private async void OpenAddBookWindow(object parameter)
        {
            try
            {
                var addBookWindow = new AddBookWindow(_apiService);

                if (addBookWindow.ShowDialog() == true)
                {
                    MessageBox.Show("Книга успешно добавлена!", "Успех!",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    await GetListBooksAsync();
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении книги! Ошибка: {ex.Message}.");
                MessageBox.Show($"Ошибка при добавлении книги!");
                throw;
            }
        }
        private async void OpenAddReaderWindow(object parameter)
        {
            try
            {
                var readerBookWindow = new AddReaderWindow(_apiService);

                if (readerBookWindow.ShowDialog() == true)
                {
                    MessageBox.Show("Читатель успешно зарегистрирован!", "Успех!",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    await GetListReadersAsync();
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при регистрации читателя! Ошибка: {ex.Message}.");
                MessageBox.Show($"Ошибка при регистрации читателя!");
                throw;
            }
        }

        private async void OpenDeleteBookWindow(object parameter)
        {
            try
            {
                if (_selectedBook == null)
                {
                    MessageBox.Show("Выберите книгу перед удалением!", "Ошибка!",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var deleteBookWindow = new DeleteBookWindow(_apiService, _selectedBook);

                if (deleteBookWindow.ShowDialog() == true)
                {
                    MessageBox.Show("Книга удалена!", "Успех!",
                                      MessageBoxButton.OK, MessageBoxImage.Information);

                    await GetListBooksAsync();
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении книги. Ошибка: {ex.Message}.");
                MessageBox.Show($"Ошибка при удалении книги!");
                throw;
            }
        }

        private async void OpenDeleteReaderWindow(object parameter)
        {
            try
            {
                if (_selectedReader == null)
                {
                    MessageBox.Show("Выберите читателя перед удалением!", "Ошибка!",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var deleteReaderWindow = new DeleteReaderWindow(_apiService, _selectedReader);

                if (deleteReaderWindow.ShowDialog() == true)
                {
                    MessageBox.Show("Читатель удалён!", "Успех!",
                                      MessageBoxButton.OK, MessageBoxImage.Information);

                    await GetListReadersAsync();
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении книги. Ошибка: {ex.Message}.");
                MessageBox.Show($"Ошибка при удалении книги!");
                throw;
            }
        }

        private async Task GetListBooksAsync()
        {
            try
            {
                var books = await _apiService.GetModelListAsync<Book>("api/books");
                _allBooks = new ObservableCollection<Book>(books);
                FilterBooks();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки списка книг: {ex.Message}.");
                MessageBox.Show("Не удалось загрузить список книг!");
            }
        }

        private async Task GetListReadersAsync()
        {
            try
            {
                var readers = await _apiService.GetModelListAsync<Reader>("api/readers");
                _allReaders = new ObservableCollection<Reader>(readers);
                FilterReaders();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке списка читателей: {ex.Message}.");
                MessageBox.Show("Не удалось загрузить список читателей!");
            }
        }

        private async Task GetListRecordsAsync()
        {
            try
            {
                var records = await _apiService.GetModelListAsync<BorrowRecord>("api/borrow_records");
                _allRecords = new ObservableCollection<BorrowRecord>(records);
                FilterRecords();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке списка записей: {ex.Message}.");
                MessageBox.Show("Не удалось загрузить список записей!");
            }
        }

        private void FilterBooks()
        {
            if (_allBooks == null) return;

            var filtered = _allBooks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(TitleFilter))
                filtered = filtered.Where(b =>
                    b.Title?.IndexOf(TitleFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(AuthorFilter))
                filtered = filtered.Where(b =>
                    b.Author?.IndexOf(AuthorFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(GenreFilter))
                filtered = filtered.Where(b =>
                    b.Genre?.IndexOf(GenreFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(IsAvailableFilter))
            {
                if (bool.TryParse(IsAvailableFilter, out bool isAvailable))
                    filtered = filtered.Where(b => b.IsAvailable == isAvailable);
            }

            Books = new ObservableCollection<Book>(filtered);
        }

        private void ResetFilters()
        {
            switch (SelectedTabIndex)
            {
                case 0:
                    ResetBookFilters();
                    break;
                case 1:
                    ResetReaderFilters();
                    break;
                case 2:
                    ResetRecordFilters();
                    break;
            }
        }

        private void FilterReaders()
        {
            if (_allReaders == null) return;

            var filtered = _allReaders.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(NameFilter))
                filtered = filtered.Where(r =>
                    r.Name?.IndexOf(NameFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrWhiteSpace(EmailFilter))
                filtered = filtered.Where(r =>
                    r.Email?.IndexOf(EmailFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            Readers = new ObservableCollection<Reader>(filtered);
        }

        private void FilterRecords()
        {
            if (_allRecords == null) return;

            var filtered = _allRecords.AsEnumerable();

            if (BookIdFilter != 0)
                filtered = filtered.Where(r => r.BookId == BookIdFilter);

            if (ReaderIdFilter != 0)
                filtered = filtered.Where(r => r.ReaderId == ReaderIdFilter);

            Records = new ObservableCollection<BorrowRecord>(filtered);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
