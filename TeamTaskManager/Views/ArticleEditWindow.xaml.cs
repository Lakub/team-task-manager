using System.Windows;
using System.Windows.Controls;

namespace TeamTaskManager.Views
{
    public partial class ArticleEditWindow : Window
    {
        public ArticleEditWindow(int? articleId, int projectId)
        {
            InitializeComponent();
            DataContext = new ViewModels.ArticleEditViewModel(articleId, projectId, () => this.DialogResult = true);
        }

        private void BtnFormat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag == null) return;
            string tag = btn.Tag.ToString();

            switch (tag)
            {
                case "Bold": InsertMarkdown("**", "**", "pogrubienie"); break;
                case "Italic": InsertMarkdown("*", "*", "kursywa"); break;
                case "Strike": InsertMarkdown("~~", "~~", "przekreślenie"); break;
                case "H1": InsertMarkdownLine("# ", "Nagłówek 1"); break;
                case "H2": InsertMarkdownLine("## ", "Nagłówek 2"); break;
                case "H3": InsertMarkdownLine("### ", "Nagłówek 3"); break;
                case "List": InsertMarkdownLine("- ", "Element listy"); break;
                case "NumList": InsertMarkdownLine("1. ", "Element listy numerowanej"); break;
                case "Task": InsertMarkdownLine("- [ ] ", "Nowe zadanie do zrobienia"); break;
                case "Quote": InsertMarkdownLine("> ", "Cytat"); break;
                case "Code": InsertMarkdown("```csharp\n", "\n```", "kod źródłowy"); break;
                case "Link": InsertMarkdown("[", "](https://link.com)", "Opis linku"); break;
                case "Image": InsertMarkdown("![", "](https://link-do-obrazka.jpg)", "Tekst alternatywny"); break;
                case "Table":
                    string tableTemplate = "\n| Kolumna 1 | Kolumna 2 |\n| --------- | --------- |\n| Wartość 1 | Wartość 2 |\n";
                    InsertMarkdown(tableTemplate);
                    break;
            }
        }

        private void InsertMarkdown(string prefix, string suffix = "", string defaultText = "")
        {
            var tb = ContentTextBox;

            int startPosition = tb.SelectionStart;

            if (tb.SelectionLength > 0)
            {
                string selected = tb.SelectedText;
                tb.SelectedText = $"{prefix}{selected}{suffix}";

                tb.SelectionStart = startPosition + prefix.Length + selected.Length + suffix.Length;
                tb.SelectionLength = 0;
            }
            else
            {
                tb.SelectedText = $"{prefix}{defaultText}{suffix}";

                tb.SelectionStart = startPosition + prefix.Length;
                tb.SelectionLength = defaultText.Length;
            }

            tb.Focus();
        }

        private void InsertMarkdownLine(string prefix, string defaultText = "")
        {
            var tb = ContentTextBox;
            int startPosition = tb.SelectionStart;

            string nl = Environment.NewLine;

            if (tb.SelectionLength > 0)
            {
                string selected = tb.SelectedText;
                tb.SelectedText = $"{nl}{prefix}{selected}{nl}";

                tb.SelectionStart = startPosition + nl.Length + prefix.Length + selected.Length + nl.Length;
                tb.SelectionLength = 0;
            }
            else
            {
                tb.SelectedText = $"{nl}{prefix}{defaultText}";

                tb.SelectionStart = startPosition + nl.Length + prefix.Length;
                tb.SelectionLength = defaultText.Length;
            }

            tb.Focus();
        }
    }
}
