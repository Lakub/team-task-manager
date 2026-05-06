using System.Windows.Controls;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class WikiMainView : UserControl
    {
        public WikiMainView(int projectId)
        {
            InitializeComponent();
            DataContext = new WikiMainViewModel(projectId);
        }
    }
}
