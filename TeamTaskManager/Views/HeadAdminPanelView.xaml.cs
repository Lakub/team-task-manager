using System.Windows;
using System.Windows.Controls;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class HeadAdminPanelView : UserControl
    {
        public HeadAdminPanelView()
        {
            InitializeComponent();
            DataContext = new HeadAdminPanelViewModel();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegisterWindow { Owner = Window.GetWindow(this) };
            if (window.ShowDialog() == true)
                ((HeadAdminPanelViewModel)DataContext).Reload();
        }
    }
}
