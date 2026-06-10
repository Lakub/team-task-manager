using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;

namespace TeamTaskManager.ViewModels
{
    public partial class EditTaskViewModel : TaskFormViewModel
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly int _taskId;

        [ObservableProperty]
        private string _taskKey = string.Empty;

        public override string WindowTitle => !string.IsNullOrEmpty(TaskKey) ? $"Edytuj zadanie: {TaskKey}" : "Edytuj zadanie";

        public EditTaskViewModel(ITaskService taskService, IProjectService projectService, int taskId)
        {
            _taskService = taskService;
            _projectService = projectService;
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

            var members = await _projectService.GetMembersByProjectIdAsync(task.ProjectId);
            ProjectMembers.Clear();
            ProjectMembers.Add(new User { Id = -1, FullName = "Nieprzypisane" });
            foreach (var m in members)
                ProjectMembers.Add(m);

            Title = task.Title;
            Description = task.Description;
            SelectedType = task.Type;
            SelectedPriority = task.Priority;

            SelectedAssignee = task.AssigneeId.HasValue
                ? ProjectMembers.FirstOrDefault(m => m.Id == task.AssigneeId.Value)
                : ProjectMembers.FirstOrDefault(m => m.Id == -1);

            TaskKey = task.Project != null ? $"{task.Project.Key}-{task.Id}" : task.Id.ToString();
            OnPropertyChanged(nameof(WindowTitle));
        }

        protected override async System.Threading.Tasks.Task ExecuteSubmitAsync()
        {
            var assigneeId = SelectedAssignee?.Id == -1 ? null : SelectedAssignee?.Id;

            await _taskService.EditTaskAsync(
                taskId: _taskId,
                title: Title.Trim(),
                description: Description.Trim(),
                type: SelectedType,
                priority: SelectedPriority,
                assigneeId: assigneeId);
        }
    }
}