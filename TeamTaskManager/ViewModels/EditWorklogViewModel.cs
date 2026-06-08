using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using TeamTaskManager.Services;

namespace TeamTaskManager.ViewModels
{
    public partial class EditWorklogViewModel : WorklogFormViewModel
    {
        private readonly IWorklogService _worklogService;
        private readonly int _worklogId;

        [ObservableProperty]
        private string _taskKey = string.Empty;

        public override string WindowTitle => !string.IsNullOrEmpty(TaskKey) ? $"Edytuj wpis: {TaskKey}" : "Edytuj wpis";

        public EditWorklogViewModel(IWorklogService worklogService, int worklogId)
        {
            _worklogService = worklogService;
            _worklogId = worklogId;
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var worklog = await _worklogService.GetWorklogByIdAsync(_worklogId);

            if (worklog == null || worklog.IsDeleted)
            {
                MessageBox.Show("Nie można znaleźć zadania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                OnCancel?.Invoke();
                return;
            }

            Description = worklog.Description;
            StartDate = worklog.StartTime;
            TimeSpentInput = worklog.TimeSpentText;

            TaskKey = $"{worklog.Task.Project.Key}-{worklog.Task.Id}";
            OnPropertyChanged(nameof(WindowTitle));
        }

        protected override async System.Threading.Tasks.Task ExecuteSubmitAsync()
        {
            await _worklogService.EditWorklogAsync(
                worklogId: _worklogId,
                description: Description.Trim(),
                startTime: StartDate,
                timeSpentText: _timeSpentParsed,
                timeSpent: TimeSpent);
        }
    }
}