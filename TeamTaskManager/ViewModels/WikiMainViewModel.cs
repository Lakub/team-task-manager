using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
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

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    LoadArticles();
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<WikiArticle> articles = new();

        // NOWE: Kolekcja dostępnych tagów w projekcie
        [ObservableProperty]
        private ObservableCollection<string> availableTags = new();


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
                    OnPropertyChanged(nameof(SelectedArticleTags));
                }
            }
        }

        public string SelectedArticleTags => SelectedArticle != null && SelectedArticle.Tags.Any()
            ? "Tagi: " + string.Join(", ", SelectedArticle.Tags.Select(t => t.Name))
            : "Brak tagów";

        public ICommand CreateNewArticleCommand { get; }
        public ICommand EditArticleCommand { get; }
        public ICommand DeleteArticleCommand { get; }

        // NOWE: Komenda do filtrowania po tagu
        public ICommand FilterByTagCommand { get; }

        public WikiMainViewModel(int projectId)
        {
            _projectId = projectId;

            CreateNewArticleCommand = new RelayCommand(CreateNewArticle);
            EditArticleCommand = new RelayCommand(EditArticle, () => SelectedArticle != null);
            DeleteArticleCommand = new RelayCommand(DeleteArticle, () => SelectedArticle != null);

            // Po kliknięciu w tag, wpisujemy go w pasek wyszukiwania (co automatycznie filtruje listę)
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
                context.Database.EnsureCreated();

                var baseQuery = context.WikiArticles
                    .Include(a => a.Tags)
                    .Where(a => a.ProjectId == _projectId);

                // NOWE: Pobieranie wszystkich unikalnych tagów użytych w artykułach dla tego projektu
                var uniqueTags = baseQuery
                    .SelectMany(a => a.Tags)
                    .Select(t => t.Name)
                    .Distinct()
                    .ToList();

                AvailableTags = new ObservableCollection<string>(uniqueTags);

                var query = baseQuery.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    var lowerQuery = SearchQuery.ToLower();
                    query = query.Where(a => a.Title.ToLower().Contains(lowerQuery) ||
                                             a.Content.ToLower().Contains(lowerQuery) ||
                                             a.Tags.Any(t => t.Name.ToLower().Contains(lowerQuery)));
                }

                Articles = new ObservableCollection<WikiArticle>(query.ToList());

                if (SelectedArticle == null && Articles.Any())
                {
                    SelectedArticle = Articles.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd odczytu bazy dokumentacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateNewArticle()
        {
            var editWindow = new ArticleEditWindow(null, _projectId);
            if (editWindow.ShowDialog() == true)
            {
                LoadArticles();
            }
        }

        private void EditArticle()
        {
            if (SelectedArticle == null) return;

            var editWindow = new ArticleEditWindow(SelectedArticle.Id, _projectId);
            if (editWindow.ShowDialog() == true)
            {
                LoadArticles();
            }
        }

        private void DeleteArticle()
        {
            if (SelectedArticle == null) return;

            var result = MessageBox.Show($"Czy na pewno chcesz usunąć artykuł '{SelectedArticle.Title}'?", "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new AppDbContext();
                    context.WikiArticles.Remove(SelectedArticle);
                    context.SaveChanges();
                    SelectedArticle = null;
                    LoadArticles();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nie udało się usunąć artykułu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}