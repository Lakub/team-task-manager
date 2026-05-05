using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{   
    public class RoleVisibilityConverter : IValueConverter
    {
        public string VisibilityLevel { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var CurrentProject = (Project)value;
            if (CurrentProject == null)
                return Visibility.Collapsed;
            //if (App.CurrentUser.OrgRole == Models.Enums.OrgRole.GigaUltraAdmin) <- tu dla giga admina się doda
            //    return Visibility.Visible;
            int lvl = int.Parse(VisibilityLevel);
            var projectUsers = CurrentProject.ProjectUsers;
            var projectUser = projectUsers.FirstOrDefault(e => e.UserId==App.CurrentUser.Id);
            if (projectUser==null)return Visibility.Collapsed;
            if ((int)projectUser.Role >= lvl)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var dbContext = new AppDbContext();
            DataContext = new MainWindowViewModel(new ProjectService(dbContext));
        }
    }
}