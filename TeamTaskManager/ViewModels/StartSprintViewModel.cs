using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;
using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.ViewModels
{
    public partial class StartSprintViewModel : ObservableValidator
    {
        private readonly IProjectService _projectService;
        private readonly int _projectId;
        private ObservableCollection<Sprint> _otherSprints = new();

        public StartSprintViewModel(IProjectService projectService, int projectId, DateTime? defaultEndDate)
        {
            _projectService = projectService;
            _projectId = projectId;
            _endDate = defaultEndDate?.Date ?? DateTime.Today.AddDays(14);

            ConfirmCommand = new AsyncRelayCommand(ExecuteConfirmAsync);
            CancelCommand = new RelayCommand(() => OnCancelled?.Invoke());
        }
        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var (project, sprints) = await _projectService.GetSprintsByProjectIdAsync(_projectId);

            _otherSprints = new ObservableCollection<Sprint>(
                sprints.Where(s => s.Status != SprintStatus.Completed));

            ValidateProperty(EndDate, nameof(EndDate));
            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(CollisionWarning));
            OnPropertyChanged(nameof(HasCollisionWarning));
        }

        // daty
        public string StartDateDisplay { get; } = DateTime.Today.ToString("dd.MM.yyyy");

        private DateTime _endDate;
        [CustomValidation(typeof(StartSprintViewModel), nameof(ValidateEndDate))]
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                SetProperty(ref _endDate, value, true);
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(CollisionWarning));
            }
        }


        // kolizja
        public string? CollisionWarning
        {
            get
            {
                var today = DateTime.Today;
                foreach (var sprint in _otherSprints)
                {
                    if (today < sprint.EndDate && EndDate > sprint.StartDate)
                        return $"Uwaga: termin pokrywa się ze sprintem \"{sprint.Name}\" ({sprint.StartDate:d} - {sprint.EndDate:d}).";
                }
                return null;
            }
        }
        public bool HasCollisionWarning => CollisionWarning != null;


        // walidacja
        public bool IsValid => !HasErrors;

        public static ValidationResult? ValidateEndDate(object? value, ValidationContext context)
        {
            var vm = (StartSprintViewModel)context.ObjectInstance;

            if (vm.EndDate.Date <= DateTime.Today)
                return new ValidationResult("Data zakończenia musi być późniejsza niż dzisiaj.");

            return ValidationResult.Success;
        }


        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public Action? OnConfirmed { get; set; }
        public Action? OnCancelled { get; set; }

        private System.Threading.Tasks.Task ExecuteConfirmAsync()
        {
            ValidateAllProperties();
            OnPropertyChanged(nameof(IsValid));

            if (IsValid)
                OnConfirmed?.Invoke();

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}