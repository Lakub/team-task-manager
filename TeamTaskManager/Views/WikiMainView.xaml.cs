using System.Windows.Controls;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class WikiMainView : UserControl
    {
        public WikiMainView()
        {
            InitializeComponent();
            DataContext = new WikiMainViewModel();
        }
    }
}
