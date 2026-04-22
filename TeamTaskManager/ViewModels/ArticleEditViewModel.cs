using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Input;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;

namespace TeamTaskManager.ViewModels
{
    public partial class ArticleEditViewModel : ObservableObject
    {
        private readonly int? _articleId;
        private readonly Action _closeAction;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string content = string.Empty;

        [ObservableProperty]
        private string tagsText = string.Empty;

        public string WindowTitle => _articleId.HasValue ? "Edytuj stronę" : "Tworzenie nowej strony";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ArticleEditViewModel(int? articleId, Action closeAction)
        {
            _articleId = articleId;
            _closeAction = closeAction;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);

            if (_articleId.HasValue)
            {
                LoadArticle(_articleId.Value);
            }
        }

        private void LoadArticle(int id)
        {
            using var context = new AppDbContext();
            var article = context.WikiArticles.Include(a => a.Tags).FirstOrDefault(a => a.Id == id);
            if (article != null)
            {
                Title = article.Title;
                Content = article.Content;
                TagsText = string.Join(", ", article.Tags.Select(t => t.Name));
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Tytuł strony jest wymagany!", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var context = new AppDbContext();
            WikiArticle article;

            if (_articleId.HasValue)
            {
                article = context.WikiArticles.Include(a => a.Tags).First(a => a.Id == _articleId.Value);
                article.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                article = new WikiArticle { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                context.WikiArticles.Add(article);
            }

            article.Title = Title;
            article.Content = Content;

            // Logika tagów: czyścimy stare i dodajemy nowe bazując na polu tekstowym
            article.Tags.Clear();
            if (!string.IsNullOrWhiteSpace(TagsText))
            {
                var tagNames = TagsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var tagName in tagNames)
                {
                    var existingTag = context.Tags.FirstOrDefault(t => t.Name.ToLower() == tagName.ToLower());
                    if (existingTag != null)
                    {
                        article.Tags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new Tag { Name = tagName };
                        context.Tags.Add(newTag);
                        article.Tags.Add(newTag);
                    }
                }
            }

            context.SaveChanges();
            _closeAction?.Invoke(); // Zamyka okno i zwraca DialogResult = true
        }

        private void Cancel()
        {
            _closeAction?.Invoke();
        }
    }
}
