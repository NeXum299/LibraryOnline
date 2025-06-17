using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAppLibrary.Models
{
    public class Book : INotifyPropertyChanged
    {
        private int id;
        private string title;
        private string author;
        private string yearPublished;
        private string genre;
        private bool isAvailable;

        public int Id
        {
            get { return id; }
            set
            {
                id = value; OnPropertyChanged(nameof(Id));
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                title = value; OnPropertyChanged(nameof(Title));
            }
        }

        public string Author
        {
            get { return author; }
            set
            {
                author = value; OnPropertyChanged(nameof(Author));
            }
        }

        public string YearPublished
        {
            get { return yearPublished; }
            set
            {
                yearPublished = value; OnPropertyChanged(nameof(YearPublished));
            }
        }

        public string Genre
        {
            get { return genre; }
            set
            {
                genre = value; OnPropertyChanged(nameof(Genre));
            }
        }

        public bool IsAvailable
        {
            get { return isAvailable; }
            set
            {
                isAvailable = value; OnPropertyChanged(nameof(IsAvailable));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
