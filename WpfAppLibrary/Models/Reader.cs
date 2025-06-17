using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAppLibrary.Models
{
    public class Reader : INotifyPropertyChanged
    {
        private int id;
        private string name;
        private string email;

        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value; OnPropertyChanged(nameof(Name));
            }
        }

        public string Email
        {
            get => email;
            set
            {
                email = value; OnPropertyChanged(nameof(Email));
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
