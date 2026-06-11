using System.Windows;
using System.Windows.Controls;

namespace TeamTaskManager.Views
{
    public partial class WikiMainView : UserControl
    {
        public WikiMainView(int projectId)
        {
            InitializeComponent();
            DataContext = new ViewModels.WikiMainViewModel(projectId);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ViewModels.WikiMainViewModel vm && e.NewValue is Models.Entities.WikiArticle article)
            {
                vm.SelectedArticle = article;
            }
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(WikiMarkdownViewer, "Eksport Artykułu Wiki");
            }
        }
    }
}