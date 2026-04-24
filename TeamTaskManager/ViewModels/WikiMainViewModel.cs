using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Views;

namespace TeamTaskManager.ViewModels
{
    public partial class WikiMainViewModel : ObservableObject
    {
 
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

        private WikiArticle? _selectedArticle;
        public WikiArticle? SelectedArticle
        {
            get => _selectedArticle;
            set
            {
                if (SetProperty(ref _selectedArticle, value))
                {

                    ((RelayCommand)EditArticleCommand).NotifyCanExecuteChanged();

                    OnPropertyChanged(nameof(SelectedArticleTags));
                }
            }
        }

        public string SelectedArticleTags => SelectedArticle != null && SelectedArticle.Tags.Any()
            ? "Tagi: " + string.Join(", ", SelectedArticle.Tags.Select(t => t.Name))
            : "Brak tagów";

        public ICommand CreateNewArticleCommand { get; }
        public ICommand EditArticleCommand { get; }

        public WikiMainViewModel()
        {
            LoadArticles();

            CreateNewArticleCommand = new RelayCommand(CreateNewArticle);
            EditArticleCommand = new RelayCommand(EditArticle, () => SelectedArticle != null);
        }

        public void LoadArticles()
        {
            using var context = new AppDbContext();
            var query = context.WikiArticles.Include(a => a.Tags).AsQueryable();

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

        private void CreateNewArticle()
        {
            var editWindow = new ArticleEditWindow(null); 
            if (editWindow.ShowDialog() == true)
            {
                LoadArticles();
            }
        }

        private void EditArticle()
        {
            if (SelectedArticle == null) return;

            var editWindow = new ArticleEditWindow(SelectedArticle.Id);
            if (editWindow.ShowDialog() == true)
            {
                LoadArticles();
            }
        }
    }
}
