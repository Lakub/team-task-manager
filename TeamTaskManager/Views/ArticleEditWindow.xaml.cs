using System.Windows;
using TeamTaskManager.ViewModels;

namespace TeamTaskManager.Views
{
    public partial class ArticleEditWindow : Window
    {
        public ArticleEditWindow(int? articleId, int projectId)
        {
            InitializeComponent();
            DataContext = new ArticleEditViewModel(articleId, projectId, () =>
            {
                this.DialogResult = true;
                this.Close();
            });
        }

        // --- FUNKCJE FORMATOWANIA MARKDOWN W EDYTORZE ---
        private void FormatText(string prefix, string suffix)
        {
            int start = ContentTextBox.SelectionStart;
            int len = ContentTextBox.SelectionLength;
            string selected = ContentTextBox.SelectedText;

            ContentTextBox.Text = ContentTextBox.Text.Remove(start, len).Insert(start, prefix + selected + suffix);
            ContentTextBox.SelectionStart = start + prefix.Length;
            ContentTextBox.SelectionLength = selected.Length;
            ContentTextBox.Focus();
        }

        private void BtnBold_Click(object sender, RoutedEventArgs e) => FormatText("**", "**");
        private void BtnItalic_Click(object sender, RoutedEventArgs e) => FormatText("*", "*");
        private void BtnH1_Click(object sender, RoutedEventArgs e) => FormatText("# ", "");
        private void BtnH2_Click(object sender, RoutedEventArgs e) => FormatText("## ", "");
        private void BtnList_Click(object sender, RoutedEventArgs e) => FormatText("- ", "");
    }
}
