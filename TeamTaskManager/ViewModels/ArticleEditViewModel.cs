using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;

namespace TeamTaskManager.ViewModels
{

    public partial class ArticleEditViewModel : ObservableValidator
    {
        private readonly int? _articleId;
        private readonly int _projectId;
        private readonly Action _closeAction;

        [ObservableProperty]
        [Required(ErrorMessage = "Tytuł strony jest wymagany!")]
        [MinLength(3, ErrorMessage = "Tytuł musi mieć min. 3 znaki!")]
        private string title = string.Empty;

        [ObservableProperty]
        private string content = string.Empty;

        [ObservableProperty]
        private string tagsText = string.Empty;

        public string WindowTitle => _articleId.HasValue ? "Edytuj stronę" : "Tworzenie nowej strony";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ArticleEditViewModel(int? articleId, int projectId, Action closeAction)
        {
            _articleId = articleId;
            _projectId = projectId;
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

            ValidateAllProperties();
            if (HasErrors)
            {
                var errors = string.Join("\n", GetErrors().Select(e => e.ErrorMessage));
                MessageBox.Show(errors, "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new AppDbContext();
                WikiArticle article;

                // Upewniamy się, że projekt o danym ID istnieje
                var project = context.Projects.FirstOrDefault(p => p.Id == _projectId);
                if (project == null)
                {
                    MessageBox.Show("Nie odnaleziono projektu powiązanego z tym artykułem.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_articleId.HasValue)
                {
                    article = context.WikiArticles.Include(a => a.Tags).First(a => a.Id == _articleId.Value);
                    article.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    article = new WikiArticle { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, ProjectId = _projectId, Project = project };
                    context.WikiArticles.Add(article);
                }

                article.Title = Title;
                article.Content = Content;

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
                _closeAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas zapisu do bazy: {ex.Message}", "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            _closeAction?.Invoke();
        }
    }
}
