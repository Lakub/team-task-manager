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
    }
}
