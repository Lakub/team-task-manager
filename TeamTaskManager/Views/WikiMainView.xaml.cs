using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TeamTaskManager.Views
{
    public partial class WikiMainView : UserControl
    {
        public WikiMainView()
        {
            InitializeComponent();
        }

        private void NewArticleButton_Click(object sender, RoutedEventArgs e)
        {
            ArticleEditWindow editWindow = new ArticleEditWindow();
            editWindow.ShowDialog();
        }

        private void EditArticleButton_Click(object sender, RoutedEventArgs e)
        {
            ArticleEditWindow editWindow = new ArticleEditWindow();
            // Tutaj w przyszłości przekażesz obiekt artykułu do edycji
            editWindow.ShowDialog();
        }
    }
}
