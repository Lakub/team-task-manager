using System.Windows;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}