using Microsoft.EntityFrameworkCore;
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
using TeamTaskManager.Helpers;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;

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

            using var context = new AppDbContext();
            //SeedData.Clear(context);
            SeedData.Seed(context);

            var dbTasks = context.Tasks
                .Include(t => t.Reporter)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .Include(t => t.Sprint)
                .Include(t => t.ParentTask)
                .ToList();

            foreach (var t in dbTasks)
                tasks.Add(t);

            TaskList.ItemsSource = tasks;

            joe = context.Users.FirstOrDefault(u => u.FullName == "Joe Mama")!;
            proj1 = context.Projects.FirstOrDefault(p => p.Name == "Projekt 1")!;
        }

        private Collection<Models.Entities.Task> tasks { get; } = new ObservableCollection<Models.Entities.Task>();

        private User joe;
        private Project proj1; 

        private void Login_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();

            context.Users.Attach(joe);
            context.Projects.Attach(proj1);

            var task = new Models.Entities.Task
            {
                Title = TaskTitleBox.Text,
                Description = TaskDescBox.Text,
                Reporter = joe,
                Project = proj1
            };

            context.Tasks.Add(task);
            context.SaveChanges();

            tasks.Add(task);
            TaskList.SelectedIndex = tasks.Count - 1;
        }
    }
}