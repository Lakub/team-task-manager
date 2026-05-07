using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;

namespace TeamTaskManager.ViewModels
{
    public partial class CreateSprintViewModel : ObservableValidator
    {
        private readonly ISprintService _sprintService;
        private readonly int _projectId;

        private ObservableCollection<Sprint> _sprints = new ObservableCollection<Sprint>();

        private string _name = string.Empty;
        [Required(ErrorMessage = "Nazwa jest wymagana.")]
        public string Name
        {
            get => _name;
            set { SetProperty(ref _name, value, true); OnPropertyChanged(nameof(IsValid)); }
        }

        private DateTime _startDate = DateTime.Today;
        [CustomValidation(typeof(CreateSprintViewModel), nameof(ValidateDates))]
        public DateTime StartDate
        {
            get => _startDate;
            set { SetProperty(ref _startDate, value, true); ValidateProperty(EndDate, nameof(EndDate)); OnPropertyChanged(nameof(IsValid)); }
        }

        private DateTime _endDate = DateTime.Today.AddDays(14);
        [CustomValidation(typeof(CreateSprintViewModel), nameof(ValidateDates))]
        public DateTime EndDate
        {
            get => _endDate;
            set { SetProperty(ref _endDate, value, true); ValidateProperty(StartDate, nameof(StartDate)); OnPropertyChanged(nameof(IsValid)); }
        }

        public bool IsValid => !HasErrors && !string.IsNullOrWhiteSpace(Name);

        [ObservableProperty]
        private bool _isBusy;

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public Action? OnSuccess { get; set; }
        public Action? OnCancel { get; set; }

        public CreateSprintViewModel(ISprintService sprintService, int projectId)
        {
            _sprintService = sprintService;
            _projectId = projectId;

            CreateCommand = new AsyncRelayCommand(ExecuteCreateAsync);
            CancelCommand = new RelayCommand(() => OnCancel?.Invoke());
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var sprints = await _sprintService.GetAllSprintsByProjectIdAsync(_projectId);
            _sprints = new ObservableCollection<Sprint>(sprints);

            ValidateProperty(StartDate, nameof(StartDate));
            ValidateProperty(EndDate, nameof(EndDate));
        }

        public static ValidationResult? ValidateDates(object? value, ValidationContext context)
        {
            var vm = (CreateSprintViewModel)context.ObjectInstance;

            if (vm.StartDate < DateTime.Today)
                return new ValidationResult("Data rozpoczęcia nie może być w przeszłości.");

            if (vm.EndDate <= vm.StartDate)
                return new ValidationResult("Data zakończenia musi być późniejsza niż rozpoczęcia.");

            foreach (var sprint in vm._sprints)
            {
                if (vm.StartDate < sprint.EndDate && vm.EndDate > sprint.StartDate)
                    return new ValidationResult($"Termin koliduje z innym sprintem: {sprint.Name} ({sprint.StartDate:d} - {sprint.EndDate:d})");
            }

            return ValidationResult.Success;
        }

        private async System.Threading.Tasks.Task ExecuteCreateAsync()
        {
            if (IsBusy) return;

            ValidateAllProperties();

            if (!IsValid)
            {
                MessageBox.Show("Nieprawidłowe dane.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;
            try
            {
                var creatorId = App.CurrentUser?.Id ?? throw new InvalidOperationException("Brak zalogowanego użytkownika.");

                await _sprintService.CreateSprintAsync(
                    name: Name.Trim(),
                    startDate: StartDate,
                    endDate: EndDate,
                    projectId: _projectId,
                    creatorId: creatorId);

                MessageBox.Show("Sprint został pomyślnie utworzony.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                OnSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas tworzenia sprintu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}