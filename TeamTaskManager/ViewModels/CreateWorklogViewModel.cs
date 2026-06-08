using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using TeamTaskManager.Services;

namespace TeamTaskManager.ViewModels
{
    public partial class CreateWorklogViewModel : WorklogFormViewModel
    {
        private readonly IWorklogService _worklogService;
        private readonly ITaskService _taskService;
        private readonly int _taskId;

        [ObservableProperty]
        private string _taskKey = string.Empty;

        public override string WindowTitle => !string.IsNullOrEmpty(TaskKey) ? $"Dodaj wpis: {TaskKey}" : "Dodaj wpis";

        public CreateWorklogViewModel(IWorklogService worklogService, ITaskService taskService, int taskId)
        {
            _worklogService = worklogService;
            _taskService = taskService;
            _taskId = taskId;
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var task = await _taskService.GetTaskByIdAsync(_taskId);

            if (task == null)
            {
                MessageBox.Show("Nie można znaleźć zadania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                OnCancel?.Invoke();
                return;
            }

            TaskKey = $"{task.Project.Key}-{task.Id}";
            OnPropertyChanged(nameof(WindowTitle));
        }

        protected override async System.Threading.Tasks.Task ExecuteSubmitAsync()
        {
            var userId = App.CurrentUser?.Id ?? throw new InvalidOperationException("Brak zalogowanego użytkownika.");

            await _worklogService.CreateWorklogAsync(
                description: Description.Trim(),
                startTime: StartDate,
                timeSpentText: _timeSpentParsed,
                timeSpent: TimeSpent,
                taskId: _taskId,
                userId: userId);
        }
    }
}