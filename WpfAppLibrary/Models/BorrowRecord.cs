using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAppLibrary.Models
{
    public class BorrowRecord : INotifyPropertyChanged
    {
        private int id;
        private int bookId;
        private int readerId;
        private DateTime borrowDate;
        private DateTime returnDate;

        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }
        public int BookId
        {
            get { return bookId; }
            set
            {
                bookId = value; OnPropertyChanged(nameof(BookId));
            }
        }

        public int ReaderId
        {
            get { return readerId; }
            set
            {
                readerId = value; OnPropertyChanged(nameof(ReaderId));
            }
        }

        public DateTime BorrowDate
        {
            get { return borrowDate; }
            set
            {
                borrowDate = value; OnPropertyChanged(nameof(BorrowDate));
            }
        }

        public DateTime ReturnDate
        {
            get { return returnDate; }
            set
            {
                returnDate = value; OnPropertyChanged(nameof(ReturnDate));
            }
        }

        public bool IsReturned { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
