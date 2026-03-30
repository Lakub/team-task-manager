using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeamTaskManager.Models;

namespace TeamTaskManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TaskList.ItemsSource = tasks;
        }

        private Collection<Models.Task> tasks { get; } = new ObservableCollection<Models.Task>();

        User tempUser = new User
        {
            FullName = "Jan Kowalski",
            Email = "user@email.com",
            Role = UserRole.Developer
        };

        private void Login_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            var task = new Models.Task
            {
                Title = TaskTitleBox.Text,
                Description = TaskDescBox.Text,
                Reporter = tempUser
            };

            tasks.Add(task);
            TaskList.SelectedIndex = tasks.Count - 1;
        }
    }
}