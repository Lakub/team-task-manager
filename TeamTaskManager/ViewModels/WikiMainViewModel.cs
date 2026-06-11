using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Views;

namespace TeamTaskManager.ViewModels
{
    public partial class WikiMainViewModel : ObservableObject
    {
        private readonly int _projectId;

        [ObservableProperty]
        private ObservableCollection<WikiArticle> rootArticles = new();

        [ObservableProperty]
        private ObservableCollection<string> availableTags = new();

        [ObservableProperty]
        private ObservableCollection<WikiArticle> breadcrumbs = new();

        [ObservableProperty]
        private ObservableCollection<TocItem> tableOfContents = new();

        [ObservableProperty]
        private bool hasToc;

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set { if (SetProperty(ref _searchQuery, value)) LoadArticles(); }
        }

        private WikiArticle? _selectedArticle;
        public WikiArticle? SelectedArticle
        {
            get => _selectedArticle;
            set
            {
                if (SetProperty(ref _selectedArticle, value))
                {
                    ((RelayCommand?)EditArticleCommand)?.NotifyCanExecuteChanged();
                    ((RelayCommand?)DeleteArticleCommand)?.NotifyCanExecuteChanged();
                    ((RelayCommand?)ToggleFavoriteCommand)?.NotifyCanExecuteChanged();

                    OnPropertyChanged(nameof(SelectedArticleTags));
                    GenerateTableOfContents();
                    UpdateBreadcrumbs();
                }
            }
        }

        public string SelectedArticleTags => SelectedArticle != null && SelectedArticle.Tags.Any()
            ? "Tagi: " + string.Join(", ", SelectedArticle.Tags.Select(t => t.Name))
            : "Brak tagów";

        public ICommand CreateNewArticleCommand { get; }
        public ICommand EditArticleCommand { get; }
        public ICommand DeleteArticleCommand { get; }
        public ICommand FilterByTagCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }

        public WikiMainViewModel(int projectId)
        {
            _projectId = projectId;

            CreateNewArticleCommand = new RelayCommand(CreateNewArticle);
            EditArticleCommand = new RelayCommand(EditArticle, () => SelectedArticle != null);
            DeleteArticleCommand = new RelayCommand(DeleteArticle, () => SelectedArticle != null);
            ToggleFavoriteCommand = new RelayCommand(ToggleFavorite, () => SelectedArticle != null);

            FilterByTagCommand = new RelayCommand<string>(tag =>
            {
                if (tag != null) SearchQuery = tag;
            });

            LoadArticles();
        }

        public void LoadArticles()
        {
            try
            {
                using var context = new AppDbContext();

                var allArticles = context.WikiArticles
                    .Include(a => a.Tags)
                    .Where(a => a.ProjectId == _projectId)
                    .ToList();

                AvailableTags = new ObservableCollection<string>(allArticles.SelectMany(a => a.Tags).Select(t => t.Name).Distinct());

                var filteredList = allArticles.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    var q = SearchQuery.ToLower();
                    filteredList = filteredList.Where(a => a.Title.ToLower().Contains(q) || a.Content.ToLower().Contains(q) || a.Tags.Any(t => t.Name.ToLower().Contains(q)));
                    RootArticles = new ObservableCollection<WikiArticle>(filteredList);
                }
                else
                {
                    RootArticles = new ObservableCollection<WikiArticle>(allArticles.Where(a => a.ParentArticleId == null).OrderByDescending(a => a.IsFavorite).ThenBy(a => a.Title));
                }

                if (SelectedArticle == null && RootArticles.Any()) SelectedArticle = RootArticles.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateBreadcrumbs()
        {
            Breadcrumbs.Clear();
            var current = SelectedArticle;
            var path = new List<WikiArticle>();

            while (current != null)
            {
                path.Add(current);
                current = current.ParentArticle;
            }
            path.Reverse();
            foreach (var item in path) Breadcrumbs.Add(item);
        }

        private void ToggleFavorite()
        {
            if (SelectedArticle == null) return;
            try
            {
                using var context = new AppDbContext();
                var article = context.WikiArticles.Find(SelectedArticle.Id);
                if (article != null)
                {
                    article.IsFavorite = !article.IsFavorite;
                    context.SaveChanges();
                    LoadArticles();
                    SelectedArticle = RootArticles.FirstOrDefault(a => a.Id == article.Id);
                }
            }
            catch { }
        }

        private void CreateNewArticle()
        {
            var editWindow = new ArticleEditWindow(null, _projectId);
            if (editWindow.ShowDialog() == true) LoadArticles();
        }

        private void EditArticle()
        {
            if (SelectedArticle == null) return;
            var editWindow = new ArticleEditWindow(SelectedArticle.Id, _projectId);
            if (editWindow.ShowDialog() == true) LoadArticles();
        }

        private void DeleteArticle()
        {
            if (SelectedArticle == null) return;
            var result = MessageBox.Show($"Zostaną usunięte również wszystkie podstrony. Czy kontynuować?", "Usuwanie", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                using var context = new AppDbContext();
                context.WikiArticles.Remove(SelectedArticle);
                context.SaveChanges();
                SelectedArticle = null;
                LoadArticles();
            }
        }

        private void GenerateTableOfContents()
        {
            TableOfContents.Clear();
            if (SelectedArticle == null || string.IsNullOrWhiteSpace(SelectedArticle.Content)) { HasToc = false; return; }

            var lines = SelectedArticle.Content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var tLine = line.TrimStart();
                if (tLine.StartsWith("#"))
                {
                    int level = tLine.TakeWhile(c => c == '#').Count();
                    if (level > 0 && level <= 3 && tLine.Length > level && tLine[level] == ' ')
                    {
                        TableOfContents.Add(new TocItem { Title = tLine.Substring(level).Trim(), Level = level });
                    }
                }
            }
            HasToc = TableOfContents.Any();
        }

        public class TocItem
        {
            public string Title { get; set; } = string.Empty;
            public int Level { get; set; }
            public Thickness Margin => new Thickness((Level - 1) * 15, 2, 0, 2);
        }
    }
}