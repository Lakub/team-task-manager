using TeamTaskManager.Models.Entities;
using TeamTaskManager.Services;

namespace TeamTaskManager.ViewModels
{
    public partial class CreateTaskViewModel : TaskFormViewModel
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly int _projectId;

        public override string WindowTitle => "Utwórz zadanie";

        public CreateTaskViewModel(ITaskService taskService, IProjectService projectService, int projectId)
        {
            _taskService = taskService;
            _projectService = projectService;
            _projectId = projectId;
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var members = await _projectService.GetMembersByProjectIdAsync(_projectId);
            ProjectMembers.Clear();
            ProjectMembers.Add(new User { Id = -1, FullName = "Nieprzypisane" });
            foreach (var m in members)
                ProjectMembers.Add(m);

            SelectedAssignee = ProjectMembers.FirstOrDefault(m => m.Id == -1);
        }

        protected override async System.Threading.Tasks.Task ExecuteSubmitAsync()
        {
            var reporterId = App.CurrentUser?.Id ?? throw new InvalidOperationException("Brak zalogowanego użytkownika.");
            var assigneeId = SelectedAssignee?.Id == -1 ? null : SelectedAssignee?.Id;

            await _taskService.CreateTaskAsync(
                title: Title.Trim(),
                description: Description.Trim(),
                type: SelectedType,
                priority: SelectedPriority,
                projectId: _projectId,
                reporterId: reporterId,
                assigneeId: assigneeId);
        }
    }
}