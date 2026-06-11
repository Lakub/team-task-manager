using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
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

      

        private string _title = string.Empty;
        [Required(ErrorMessage = "Tytuł jest wymagany!")]
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }

        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        private string _tagsText = string.Empty;
        public string TagsText
        {
            get => _tagsText;
            set => SetProperty(ref _tagsText, value);
        }

        private bool _isDraft;
        public bool IsDraft
        {
            get => _isDraft;
            set => SetProperty(ref _isDraft, value);
        }

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }

        private ObservableCollection<WikiArticle> _availableParents = new();
        public ObservableCollection<WikiArticle> AvailableParents
        {
            get => _availableParents;
            set => SetProperty(ref _availableParents, value);
        }

        private WikiArticle? _selectedParent;
        public WikiArticle? SelectedParent
        {
            get => _selectedParent;
            set => SetProperty(ref _selectedParent, value);
        }


        public string WindowTitle => _articleId.HasValue ? "Edytuj stronę" : "Tworzenie nowej strony";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ApplyTemplateCommand { get; }

        public ArticleEditViewModel(int? articleId, int projectId, Action closeAction)
        {
            _articleId = articleId;
            _projectId = projectId;
            _closeAction = closeAction;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(() => _closeAction?.Invoke());
            ApplyTemplateCommand = new RelayCommand<string>(ApplyTemplate);

            LoadParents();

            if (_articleId.HasValue)
            {
                LoadArticle(_articleId.Value);
            }
        }

        private void LoadParents()
        {
            using var context = new AppDbContext();
            var parents = context.WikiArticles.Where(a => a.ProjectId == _projectId && a.Id != _articleId).ToList();

            AvailableParents = new ObservableCollection<WikiArticle>(parents);

            AvailableParents.Insert(0, new WikiArticle { Id = 0, Title = "-- Brak (Katalog Główny) --" });
            SelectedParent = AvailableParents.First();
        }

        private void LoadArticle(int id)
        {
            using var context = new AppDbContext();
            var article = context.WikiArticles.Include(a => a.Tags).FirstOrDefault(a => a.Id == id);
            if (article != null)
            {
                Title = article.Title;
                Content = article.Content;
                IsDraft = article.IsDraft;
                IsFavorite = article.IsFavorite;
                TagsText = string.Join(", ", article.Tags.Select(t => t.Name));

                if (article.ParentArticleId.HasValue)
                {
                    SelectedParent = AvailableParents.FirstOrDefault(p => p.Id == article.ParentArticleId.Value);
                }
            }
        }

        private void ApplyTemplate(string? templateType)
        {
            if (!string.IsNullOrWhiteSpace(Content))
            {
                if (MessageBox.Show("Zastosowanie szablonu nadpisze aktualną treść edytora. Czy kontynuować?", "Ostrzeżenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;
            }

            switch (templateType)
            {
                case "API":
                    Content = "# Endpoint API: [Nazwa]\n\n**Metoda:** GET/POST \n**Ścieżka:** `/api/...`\n\n## Opis\n...\n\n## Request\n```json\n{\n}\n```\n\n## Response\n```json\n{\n}\n```";
                    break;
                case "Meeting":
                    Content = "# Notatka ze spotkania\n\n**Data:** \n**Uczestnicy:** \n\n## Agenda\n1. \n2. \n\n## Ustalenia\n- \n- \n\n## Akcje do podjęcia (To-Do)\n- [ ] Zadanie 1";
                    break;
                case "Bug":
                    Content = "# Raport Błędu\n\n## Środowisko\n- Wersja:\n- OS:\n\n## Kroki do reprodukcji\n1. \n2. \n\n## Spodziewany wynik\n...\n\n## Aktualny wynik\n...\n";
                    break;
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

                if (_articleId.HasValue)
                {
                    article = context.WikiArticles.Include(a => a.Tags).First(a => a.Id == _articleId.Value);
                    article.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    article = new WikiArticle { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, ProjectId = _projectId };
                    context.WikiArticles.Add(article);
                }

                article.Title = Title;
                article.Content = Content;
                article.IsDraft = IsDraft;
                article.IsFavorite = IsFavorite;

                article.ParentArticleId = (SelectedParent != null && SelectedParent.Id != 0) ? SelectedParent.Id : null;

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
                MessageBox.Show($"Błąd podczas zapisu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}