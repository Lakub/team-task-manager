using System.Windows;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class ArticleEditWindow : Window
    {
        public ArticleEditWindow(int? articleId) 
        {
            InitializeComponent();
            DataContext = new ArticleEditViewModel(articleId, () =>
            {
                this.DialogResult = true;
                this.Close();
            });
        }
    }
}
