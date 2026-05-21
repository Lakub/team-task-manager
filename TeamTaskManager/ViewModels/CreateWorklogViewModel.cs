using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TeamTaskManager.Helpers;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using TeamTaskManager.Services;

namespace TeamTaskManager.ViewModels
{
    public partial class CreateWorklogViewModel : ObservableValidator
    {
        private readonly IWorklogService _worklogService;
        private readonly ITaskService _taskService;
        private readonly int _taskId;

        public string WindowTitle => !string.IsNullOrEmpty(TaskKey) ? $"Loguj pracę: {TaskKey}" : "Loguj pracę";

        [ObservableProperty]
        private string _taskKey = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private TimeSpan _timeSpent;

        private DateTime _startDate = DateTime.Today;
        [CustomValidation(typeof(CreateWorklogViewModel), nameof(ValidateStartDate))]
        public DateTime StartDate
        {
            get => _startDate;
            set {
                SetProperty(ref _startDate, value, true);
                _startHour = _startDate.Hour;
                _startMinute = _startDate.Minute;
                OnPropertyChanged(nameof(StartHour));
                OnPropertyChanged(nameof(StartMinute));
                ValidateProperty(TimeSpentInput, nameof(TimeSpentInput));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private int _startHour = DateTime.Now.Hour;
        public string StartHour
        {
            get => _startHour.ToString("D2");
            set
            {
                if (int.TryParse(value, out var h))
                    SetProperty(ref _startHour, Math.Clamp(h, 0, 23));

                OnPropertyChanged();
                UpdateStartDate();
            }
        }

        private int _startMinute = 0;
        public string StartMinute
        {
            get => _startMinute.ToString("D2");
            set
            {
                if (int.TryParse(value, out var m))
                    SetProperty(ref _startMinute, Math.Clamp(m, 0, 59));

                OnPropertyChanged();
                UpdateStartDate();
            }
        }

        private void UpdateStartDate()
        {
            StartDate = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, _startHour, _startMinute, 0);
        }

        private string _timeSpentInput = string.Empty;
        [Required(ErrorMessage = "Czas spędzony jest wymagany.")]
        [CustomValidation(typeof(CreateWorklogViewModel), nameof(ValidateTimeSpentInput))]
        public string TimeSpentInput
        {
            get => _timeSpentInput;
            set
            {
                SetProperty(ref _timeSpentInput, value, true);
                if (TimeSpanExtensions.TryParse(_timeSpentInput, out var timeSpent))
                    TimeSpent = timeSpent;

                ValidateProperty(_timeSpentInput, nameof(TimeSpentInput));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(TimeSpentInput) && !HasErrors;

        [ObservableProperty]
        private bool _isBusy;

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public Action? OnSuccess { get; set; }
        public Action? OnCancel { get; set; }


        public CreateWorklogViewModel(IWorklogService worklogService, ITaskService taskService, int taskId)
        {
            _worklogService = worklogService;
            _taskService = taskService;
            _taskId = taskId;

            CreateCommand = new AsyncRelayCommand(ExecuteCreateAsync);
            CancelCommand = new RelayCommand(() => OnCancel?.Invoke());
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var task = await _taskService.GetTaskByIdAsync(_taskId);

            if (task == null)
            {
                MessageBox.Show("Nie można znaleźć zadania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TaskKey = $"{task.Project.Key}-{task.Id}";

            OnPropertyChanged(nameof(WindowTitle));
        }

        public static ValidationResult? ValidateStartDate(DateTime startDate, ValidationContext context)
        {
            if (startDate > DateTime.Now)
                return new ValidationResult("Data rozpoczęcia nie może być w przyszłości.");

            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateTimeSpentInput(string input, ValidationContext context)
        {
            if (!TimeSpanExtensions.TryParse(input, out var timeSpent))
                return new ValidationResult("Nieprawidłowy format czasu.");

            if (timeSpent <= TimeSpan.Zero)
                return new ValidationResult("Czas spędzony musi być większy od zera.");

            var vm = (CreateWorklogViewModel)context.ObjectInstance;
            var endTime = vm.StartDate.Add(timeSpent);
            if (endTime > DateTime.Now)
                return new ValidationResult($"Czas zakończenia nie może być w przyszłości ({endTime:dd.MM.yyyy HH:mm}).");

            return ValidationResult.Success;
        }

        private async System.Threading.Tasks.Task ExecuteCreateAsync()
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
                var userId = App.CurrentUser?.Id ?? throw new InvalidOperationException("Brak zalogowanego użytkownika.");

                await _worklogService.CreateWorklogAsync(
                    description: Description.Trim(),
                    startTime: StartDate,
                    timeSpent: TimeSpent,
                    taskId: _taskId,
                    userId: userId);

                MessageBox.Show("Pomyślnie zalogowano pracę.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                OnSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas logowania pracy: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}