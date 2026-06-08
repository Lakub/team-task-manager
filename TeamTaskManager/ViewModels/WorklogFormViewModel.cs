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
    public abstract partial class WorklogFormViewModel : ObservableValidator
    {
        public abstract string WindowTitle { get; }

        [ObservableProperty]
        private string _taskKey = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private TimeSpan _timeSpent;

        private DateTime _startDate = DateTime.Today;
        [CustomValidation(typeof(WorklogFormViewModel), nameof(ValidateStartDate))]
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

        protected string _timeSpentParsed = string.Empty;

        private string _timeSpentInput = string.Empty;
        [Required(ErrorMessage = "Czas spędzony jest wymagany.")]
        [CustomValidation(typeof(WorklogFormViewModel), nameof(ValidateTimeSpentInput))]
        public string TimeSpentInput
        {
            get => _timeSpentInput;
            set
            {
                SetProperty(ref _timeSpentInput, value, true);
                if (TimeSpanExtensions.TryParse(_timeSpentInput, out var timeSpent, out _timeSpentParsed))
                    TimeSpent = timeSpent;

                ValidateProperty(_timeSpentInput, nameof(TimeSpentInput));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(TimeSpentInput) && !HasErrors;

        [ObservableProperty]
        private bool _isBusy;

        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        public Action? OnSuccess { get; set; }
        public Action? OnCancel { get; set; }


        protected WorklogFormViewModel()
        {
            SubmitCommand = new AsyncRelayCommand(ExecuteSubmitCoreAsync);
            CancelCommand = new RelayCommand(() => OnCancel?.Invoke());
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

            var vm = (WorklogFormViewModel)context.ObjectInstance;
            var endTime = vm.StartDate.Add(timeSpent);
            if (endTime > DateTime.Now)
                return new ValidationResult($"Czas zakończenia nie może być w przyszłości ({endTime:dd.MM.yyyy HH:mm}).");

            return ValidationResult.Success;
        }

        private async System.Threading.Tasks.Task ExecuteSubmitCoreAsync()
        {
            if (IsBusy) return;

            if (!IsValid)
            {
                MessageBox.Show("Nieprawidłowe dane.", "Ostrzeżenie", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected abstract System.Threading.Tasks.Task ExecuteSubmitAsync();
    }
}