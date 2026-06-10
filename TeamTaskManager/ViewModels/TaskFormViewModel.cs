using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.ViewModels
{
    public abstract partial class TaskFormViewModel : ObservableValidator
    {
        public abstract string WindowTitle { get; }

        private string _title = string.Empty;
        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        public string Title
        {
            get => _title;
            set { SetProperty(ref _title, value, true); OnPropertyChanged(nameof(IsValid)); }
        }

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private TaskType _selectedType = TaskType.Feature;

        [ObservableProperty]
        private TaskPriority _selectedPriority = TaskPriority.Medium;

        [ObservableProperty]
        private User? _selectedAssignee;

        public ObservableCollection<User> ProjectMembers { get; } = new();
        public ObservableCollection<TaskType> TaskTypes { get; } = new(Enum.GetValues(typeof(TaskType)).Cast<TaskType>());
        public ObservableCollection<TaskPriority> Priorities { get; } = new(Enum.GetValues(typeof(TaskPriority)).Cast<TaskPriority>());

        public bool IsValid => !string.IsNullOrWhiteSpace(Title) && !HasErrors;

        [ObservableProperty]
        private bool _isBusy;

        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        public Action? OnSuccess { get; set; }
        public Action? OnCancel { get; set; }

        protected TaskFormViewModel()
        {
            SubmitCommand = new AsyncRelayCommand(ExecuteSubmitCoreAsync);
            CancelCommand = new RelayCommand(() => OnCancel?.Invoke());
        }

        private async System.Threading.Tasks.Task ExecuteSubmitCoreAsync()
        {
            if (IsBusy) return;

            if (!IsValid)
            {
                MessageBox.Show("Nieprawidłowe dane.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;
            try
            {
                await ExecuteSubmitAsync();
                OnSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas zapisywania zadania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected abstract System.Threading.Tasks.Task ExecuteSubmitAsync();
    }
}