using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using WpfAppLibrary.View;
using WpfAppLibrary.ViewModel;

namespace WpfAppLibrary
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            HttpClient httpClient = new HttpClient();
            string baseUrl = "https://localhost:7058";
            LibraryApiService apiService = new LibraryApiService(httpClient, baseUrl);

            DataContext = new MainWindowViewModel(apiService);
        }
    }
}
