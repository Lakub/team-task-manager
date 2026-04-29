using System.Windows;
using TeamTaskManager.Models;
using TeamTaskManager.ViewModels;
using TeamTaskManager.Services;

namespace TeamTaskManager.Views
{    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var dbContext = new AppDbContext();
            DataContext = new MainWindowViewModel(new ProjectService(dbContext));
        }
    }
}