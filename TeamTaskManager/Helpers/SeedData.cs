using System;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;

namespace TeamTaskManager.Helpers
{
    public static class SeedData
    {
        public static void Seed(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any()) return;

            var user1 = new User { FullName = "Jan Kowalski", Email = "j.kowalski@email.com" };
            var user2 = new User { FullName = "Kamil Slimak", Email = "k.slimak@email.com" };
            var user3 = new User { FullName = "Joe Mama", Email = "j.mama@email.com" };
            context.Users.AddRange(user1, user2, user3);

            var project = new Project
            {
                Name = "Projekt 1",
                Description = "Projekt z seeda",
                Owner = user1
            };
            context.Projects.Add(project);

            context.ProjectUsers.Add(new ProjectUser { Project = project, User = user1, Role = UserRole.Manager });
            context.ProjectUsers.Add(new ProjectUser { Project = project, User = user2, Role = UserRole.Developer });
            context.ProjectUsers.Add(new ProjectUser { Project = project, User = user3, Role = UserRole.Developer });

            var taskCreationDate = DateTime.UtcNow.AddDays(-15);
            var sprintCreationDate = DateTime.UtcNow.AddDays(-10);
            var sprintStartDate = DateTime.UtcNow.AddDays(-7);
            var sprintEndDate = DateTime.UtcNow.AddDays(7);

            var sprint = new Sprint
            {
                Name = "Sprint 1",
                StartDate = sprintStartDate,
                EndDate = sprintEndDate,
                Status = SprintStatus.Active,
                Project = project,
                Creator = user1
            };
            context.Sprints.Add(sprint);

            var task1 = new Models.Entities.Task
            {
                Title = "zrobeinie klonu Jira",
                Description = "zrobienie zarzadzania projektami, sprintami i zadaniami",
                Reporter = user1,
                Assignee = user3,
                Status = Models.Enums.TaskStatus.InProgress,
                Priority = TaskPriority.High,
                Project = project,
                CreatedAt = taskCreationDate
                // Sprint = sprint
            };
            var task2 = new Models.Entities.Task
            {
                Title = "seedowanie db",
                Description = "klasa SeedData z .Clear i .Seed",
                Reporter = user2,
                Assignee = user2,
                Status = Models.Enums.TaskStatus.Closed,
                Priority = TaskPriority.Low,
                Project = project,
                CreatedAt = taskCreationDate
                // Sprint = sprint
            };
            var task3 = new Models.Entities.Task
            {
                Title = "zaktualizowac diagramy uml",
                Reporter = user1,
                Project = project,
                CreatedAt = taskCreationDate
            };
            var task4 = new Models.Entities.Task
            {
                Title = "naprawic merge #21",
                Description = "bo kamil znowu cos zepsul",
                Reporter = user1,
                Assignee = user1,
                Type = TaskType.Bug,
                Project = project,
                CreatedAt = taskCreationDate
                // Sprint = sprint
            };
            var task5 = new Models.Entities.Task
            {
                Title = "zrobienie podstawowego navbara",
                Description = "logowanko itp",
                Reporter = user3,
                Assignee = user3,
                Project = project,
                CreatedAt = taskCreationDate,
                // Sprint = sprint,
                ParentTask = task1
            };
            context.Tasks.AddRange(task1, task2, task3, task4, task5);

            var sprintTask1 = new SprintTask
            {
                Sprint = sprint,
                Task = task1,
                AddedAt = sprintCreationDate,
                AddedBy = user1,
                Status = task1.Status,      // InProgress
                Assignee = task1.Assignee,  // user3
            };
            var sprintTask2 = new SprintTask
            {
                Sprint = sprint,
                Task = task2,
                AddedAt = sprintCreationDate,
                AddedBy = user1,
                Status = task2.Status,      // Closed
                Assignee = task2.Assignee   // user2
            };
            var sprintTask3 = new SprintTask
            {
                Sprint = sprint,
                Task = task3,
                AddedAt = sprintCreationDate,
                AddedBy = user1,
                Status = task3.Status,              // Open
                Assignee = user2,
                RemovedAt = sprintStartDate.AddDays(4), // deprecated
                RemovedBy = user1               
            };
            var sprintTask4 = new SprintTask
            {
                Sprint = sprint,
                Task = task4,
                AddedAt = sprintCreationDate,
                AddedBy = user1,
                Status = task4.Status,      // Open
                Assignee = task4.Assignee   // user1
            };
            var sprintTask5 = new SprintTask
            {
                Sprint = sprint,
                Task = task5,
                AddedAt = sprintStartDate.AddDays(1),    // dodane pozniej
                AddedBy = user1,
                Status = task5.Status,                  // Open
                Assignee = task5.Assignee               // user3
            };
            context.SprintTasks.AddRange(sprintTask1, sprintTask2, sprintTask3, sprintTask4, sprintTask5);

            var worklog1 = new Worklog
            {
                Task = task2,
                User = user2,
                Description = "zrobiono basic funkcje Seed",
                StartTime = sprintStartDate.AddDays(2).AddHours(-2),
                TimeSpent = TimeSpan.FromHours(2),
                LoggedAt = sprintStartDate.AddDays(2),
            };
            var worklog2 = new Worklog
            {
                Task = task1,
                User = user3,
                Description = "dodano klasy modeli",
                StartTime = sprintStartDate.AddDays(3).AddHours(-2),
                TimeSpent = TimeSpan.FromHours(2),
                LoggedAt = sprintStartDate.AddDays(3),
            };
            var worklog3 = new Worklog
            {
                Task = task2,
                User = user2,
                Description = "zrobiono basic funkcje Clear",
                StartTime = sprintStartDate.AddDays(5).AddHours(-1),
                TimeSpent = TimeSpan.FromHours(1),
                LoggedAt = sprintStartDate.AddDays(5),
            };
            context.Worklogs.AddRange(worklog1, worklog2, worklog3);

            var comment1 = new Comment
            {
                Task = task1,
                Commenter = user3,
                Text = "literowka w nazwie jest"
            };

            var comment2 = new Comment
            {
                Task = task1,
                Commenter = user2,
                Text = "oops faktycznie mb",
                ParentComment = comment1
            };

            context.SaveChanges();
        }

        public static void Clear(AppDbContext context)
        {
            context.Database.EnsureDeleted();
        }
    }
}