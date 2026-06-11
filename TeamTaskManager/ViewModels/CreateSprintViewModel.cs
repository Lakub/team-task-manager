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

        private ObservableCollection<Sprint> _sprints = new();

        private string _name = string.Empty;
        [Required(ErrorMessage = "Nazwa jest wymagana.")]
        public string Name
        {
            get => _name;
            set { SetProperty(ref _name, value, true); OnPropertyChanged(nameof(IsValid)); }
        }


        // daty
        private DateTime? _startDate = DateTime.Today;
        [CustomValidation(typeof(CreateSprintViewModel), nameof(ValidateDates))]
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                SetProperty(ref _startDate, value, true);
                ValidateProperty(EndDate, nameof(EndDate));
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(CollisionWarning));
                OnPropertyChanged(nameof(HasCollisionWarning));
            }
        }

        private DateTime? _endDate = DateTime.Today.AddDays(14);
        [CustomValidation(typeof(CreateSprintViewModel), nameof(ValidateDates))]
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                SetProperty(ref _endDate, value, true);
                ValidateProperty(StartDate, nameof(StartDate));
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(CollisionWarning));
                OnPropertyChanged(nameof(HasCollisionWarning));
            }
        }


        // kolizja
        public string? CollisionWarning
        {
            get
            {
                if (StartDate == null || EndDate == null) return null;

                foreach (var sprint in _sprints)
                {
                    if (StartDate.Value < sprint.EndDate && EndDate.Value > sprint.StartDate)
                        return $"Uwaga: termin pokrywa się ze sprintem \"{sprint.Name}\" ({sprint.StartDate:d} - {sprint.EndDate:d}).";
                }
                return null;
            }
        }

        public bool HasCollisionWarning => CollisionWarning != null;

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

            _startDate = DateTime.Today;
            _endDate = DateTime.Today.AddDays(14);

            CreateCommand = new AsyncRelayCommand(ExecuteCreateAsync);
            CancelCommand = new RelayCommand(() => OnCancel?.Invoke());
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var sprints = await _sprintService.GetAllSprintsByProjectIdAsync(_projectId);
            _sprints = new ObservableCollection<Sprint>(sprints);

            if (StartDate != null) ValidateProperty(StartDate, nameof(StartDate));
            if (EndDate != null) ValidateProperty(EndDate, nameof(EndDate));
        }

        public static ValidationResult? ValidateDates(object? value, ValidationContext context)
        {
            var vm = (CreateSprintViewModel)context.ObjectInstance;

            if (vm.StartDate == null && vm.EndDate == null)
                return ValidationResult.Success;

            if (vm.StartDate == null || vm.EndDate == null)
                return new ValidationResult("Podaj obie daty lub żadnej.");

            if (vm.StartDate.Value.Date < DateTime.Today)
                return new ValidationResult("Data rozpoczęcia nie może być w przeszłości.");

            if (vm.EndDate.Value.Date <= vm.StartDate.Value.Date)
                return new ValidationResult("Data zakończenia musi być późniejsza niż rozpoczęcia.");

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