using System;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using User = TeamTaskManager.Models.Entities.User;
using Task = TeamTaskManager.Models.Entities.Task;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;
using TeamTaskManager.Services;

namespace TeamTaskManager.Helpers
{
    public static class SeedData
    {
        public static bool IsSeeded(AppDbContext context)
        {
            return context.UserAuths.Any();
        }

        // DEPRECATED
        public static void Seed(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (IsSeeded(context))
            {
                return;
            }

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

            var task1 = new Task
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
            var task2 = new Task
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
            var task3 = new Task
            {
                Title = "zaktualizowac diagramy uml",
                Reporter = user1,
                Project = project,
                CreatedAt = taskCreationDate
            };
            var task4 = new Task
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
            var task5 = new Task
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

            
            if (!context.WikiArticles.Any())
            {
                var article = new WikiArticle
                {
                    Title = "Architektura Systemu",
                    Content = "System opiera się na wzorcu MVVM z wykorzystaniem Entity Framework Core i SQLite. Dokumentacja jest dynamicznie ładowana z bazy danych.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var tag = new Tag { Name = "Dokumentacja" };
                article.Tags.Add(tag);
                context.WikiArticles.Add(article);
            }

            
            context.SaveChanges();
        }

        public static void RandomSeed(AppDbContext context, IAuthService authService, int numUsers = 20, int numProjects = 3, int minNumUsersPerProject = 5, int maxNumUsersPerProject = 9, int minNumSprintsPerProject = 3, int maxNumSprintsPerProject = 7, int minNumTasksPerProject = 20, int maxNumTasksPerProject = 30, int numProjectOwners = 2, int minNumManagersPerProject = 1, int maxNumManagersPerProject = 2, int sprintLength = 14)
        {
            context.Database.EnsureCreated();

            if (IsSeeded(context))
            {
                return;
            }

            // uzytkownicy

            var users = new List<User>();

            List<string> FirstNameOptions = new List<string>
            {
                "Aldona", "Teodor", "Rudolf", "Róża", "Hanna", "Zbigniew", "Bożena", "Barbara", "Władysława", "Filip", "Eryk", "Leon", "Mikołaj", "Dominik", "Bartosz", "Stefan", "Alicja", "Fryderyk", "Borys", "Lena", "Milena", "Róża", "Eugenia", "Wiesław", "Daria", "Wiktoria", "Irena", "Tomasz", "Romuald", "Aleksander", "Zuzanna", "Daria", "Dominika", "Róża", "Klara", "Małgorzata", "Nataliia", "Genowefa", "Bernard", "Sylwester"
            };

            List<string> LastNameOptions = new List<string>
            {
                "Rudowicz", "Stępak", "Łoziński", "Włodarz", "Frelek", "Zavhorodnia", "Prokopchuk", "Radwański", "Środa", "Zając", "Małkiewicz", "Zębik", "Przerwa", "Olko", "Lemieszek", "Bronikowska", "Motyka", "Borcz", "Kubina", "Żuczek", "Ciochoń", "Powązka", "Dudczak", "Myszka", "Galas", "Kęska", "Talarowska", "Biegała", "Mytnik", "Chwalisz", "Hydzik", "Prądziński", "Tarnowska", "Szwejkowska", "Gaweł", "Pokojski", "Płoszaj", "Kruszczyński", "Poterek", "Kopel"
            };

            List<string> EmailDomainOptions = new List<string>
            {
                "gmail.com", "outlook.com", "proton.me", "hotmail.com", "wp.pl", "onet.pl", "interia.pl", "yahoo.com"
            };

            // nie chcemy by sie nazwiska powtarzaly bo zagmatwane bedzie robienie e-maili
            var LastNames = LastNameOptions.OrderBy(x => Guid.NewGuid()).Take(numUsers).ToList();

            for (int i = 0; i < numUsers; i++)
            {
                var FirstName = FirstNameOptions[Random.Shared.Next(FirstNameOptions.Count)];
                var LastName = LastNames[i];
                if (FirstName.Last() == 'a' && (LastName.EndsWith("ski") || LastName.EndsWith("cki") || LastName.EndsWith("dzki")))
                {
                    LastName = LastName[..^1] + "a";
                }
                else if (FirstName.Last() != 'a' && (LastName.EndsWith("ska") || LastName.EndsWith("cka") || LastName.EndsWith("dzka")))
                {
                    LastName = LastName[..^1] + "i";
                }
                var FullName = $"{FirstName} {LastName}";
                var EmailDomain = EmailDomainOptions[Random.Shared.Next(EmailDomainOptions.Count)];
                var FirstNameNoDiacritics = FirstName.RemoveDiacritics();
                var LastNameNoDiacritics = LastName.RemoveDiacritics();
                var Email = $"{FirstNameNoDiacritics.ToLower()}.{LastNameNoDiacritics.ToLower()}@{EmailDomain}";
                var CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(40, 50));
                var LastLogin = DateTime.UtcNow.AddHours(-Random.Shared.Next(2, 48));
                users.Add(new User
                {
                    FullName = FullName,
                    Email = Email,
                    CreatedAt = CreatedAt,
                    LastLogin = LastLogin
                });
            }
            context.Users.AddRange(users);
            context.SaveChanges(); // aby Id bylo potem do filtrowania


            // auth

            var userAuths = new List<UserAuth>();

            foreach (var user in context.Users)
            {
                var salt = Guid.NewGuid().ToString();
                userAuths.Add(new UserAuth
                {
                    UserId = user.Id,
                    PasswordSalt = salt,
                    Password = authService.HashPassword("password", salt)
                });
            }

            context.UserAuths.AddRange(userAuths);


            // projekty

            var projects = new List<Project>();

            var Adjectives = new List<string>
            {
                "przykładny", "roztoczowy", "kosmaty", "cętkowany", "marudny", "doroczny", "szefowski", "bilateralny", "niestanowczy", "hagiograficzny", "ważny", "stacyjny", "bezskuteczny", "areligijny", "cieniutki", "achromatyczny", "zasiadający", "lichwiarski", "postawny", "dzielnicowy", "fatalistyczny", "donośny", "buchasty", "starowierczy", "radioliniowy", "naftowy", "metrowy", "łukowy", "generacyjny", "szalbierczy", "żywy", "stalisty", "inżynieryjny", "klęskowy", "nieukojony", "wędrujący", "rozmoczony", "fonetyczny", "węglonośny", "kontuszowy", "uparty", "nieciągły", "piskliwy", "gruziński", "przepadły", "korbowy", "nieszczelny", "peremptoryczny", "dojrzały", "niegospodarny", "szosowy", "bezideowy", "zadziorny", "uliczny", "potajemny", "bombonierkowy", "mierzalny", "rajski", "letargiczny", "wpółprzytomny", "stanisławowski", "wielokształtny", "rdzeniowy", "elektrolityczny", "płochliwy", "leżący", "cyfrowy", "pełnomocny", "rozlewny", "piracki", "spirytusowy", "niezasadny", "członowy", "nienaruszalny", "niebujny", "niepohamowany", "przedmiejski", "rzeczownikowy", "efektowny", "miedziowy", "okazjonalny", "radiacyjny", "niepilny", "niepalny", "pokazowy", "prawoskrętny", "zabłąkany", "wyboisty", "lwi", "niesamodzielny", "cielesny", "ósmy", "łapczywy", "sekretny", "składkowy", "niestateczny", "wątpiący", "pulchny", "rytmiczny", "dodatkowy"
            };

            var Nouns = new List<string>
            {
                "tytuł", "dźwięk", "szybkość", "chrząszcz", "okładka", "opieka", "kawałek", "kurczak", "małpa", "pociąg", "mgła", "szansa", "ciągnięcie", "wiadro", "nożyczki", "włosy", "ślimak", "wiara", "paczka", "deszcz ze śniegiem", "brama", "pchnięcie", "cent", "głos", "robak", "kopia", "ptak", "błąd", "dostosowanie", "porozumienie", "niemowlę", "powietrze", "kabel", "korzeń", "piątka", "wujek", "sztuczka", "meduza", "zupa", "zwierzę domowe", "palec", "strach", "bicz", "oszust", "ciecz", "moc", "rzeka", "narodziny", "igła", "park", "przyjaciel", "banan", "przyjemność", "śmiech", "religia", "broda", "piasek", "torba", "koszykówka", "impuls", "wzgórze", "człowiek", "pomiar", "jednostka", "humor", "koralik", "mycie", "dach", "kieszeń", "dziura", "zoo", "papier", "chłopak", "budynek", "pszczoła", "baza", "ignam", "deska", "firma", "cynk", "miedź", "ślimak", "pocałunek", "palenie", "użycie", "liść", "dzwon", "ból", "sałata", "łyżwa", "ciało", "próba", "zebra", "jedwab", "marnotrawstwo", "prześcieradło", "zespół", "urodziny", "zamówienie", "przestępstwo"
            };

            var Verbs = new List<string>
            {
                "bredzić", "wzmagać", "czepiać", "budżetować", "dręczyć", "fiksować", "meblować", "honorować", "chromować", "stawać", "skaleczyć", "hołdować", "aportować", "salwować", "czytać", "bzykać", "konfigurować", "teleportować", "dumać", "kosztować", "ubolewać", "barwić", "dorwać", "demoralizować", "chlubić", "cmokać", "alokować", "degustować", "dziadować", "bajdurzyć", "bryknąć", "chałturzyć", "biwakować", "akompaniować", "dryfować", "dopompować", "grodzić", "kochać", "obalać", "dokrajać", "doprowadzić", "stykać", "zwiedzać", "balować", "definiować", "dybać", "dodrukować", "badać", "zwilżać", "obfotografować", "cedzić", "harować", "bełkotać", "zeswatać", "boleć", "golnąć", "bąblować", "obadać", "ignorować", "dopowiadać", "restaurować", "pisać", "mówić", "robić", "jeść", "pić", "spać", "biegać", "chodzić", "myśleć", "widzieć", "słyszeć", "czuć", "tworzyć", "niszczyć", "kupować", "sprzedawać", "otwierać", "zamykać", "zaczynać", "kończyć", "uczyć", "zapominać", "pamiętać", "szukać", "znajdować", "tracić", "zyskiwać", "budować", "burzyć", "prowadzić", "zatrzymywać", "wysyłać", "odbierać", "tłumaczyć", "sprawdzać", "liczyć", "mierzyć", "ważyć", "drukować"
            };

            var LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

            // kilka userow aby bylo wlascicielami projektow
            var projectOwners = users.Take(numProjectOwners).ToList();

            for (int i = 0; i < numProjects; i++)
            {
                var pNoun = Nouns[Random.Shared.Next(Nouns.Count)];
                var pAdjective = Adjectives[Random.Shared.Next(Adjectives.Count)];
                pAdjective = pAdjective.OdmienPrzymiotnik(pNoun);
                var pName = $"{pAdjective} {pNoun}";
                var pDescription = LoremIpsum.Substring(0, Random.Shared.Next(50, LoremIpsum.Length));
                var pOwner = projectOwners[Random.Shared.Next(projectOwners.Count)];
                var pCreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(40, 50));
                var project = new Project
                {
                    Name = pName,
                    Description = pDescription,
                    Owner = pOwner,
                    CreatedAt = pCreatedAt
                };
                projects.Add(project);
                context.Projects.Add(project);


                // project users

                var projectUsers = new List<ProjectUser>
                {
                    new ProjectUser
                    {
                        Project = project,
                        User = pOwner,
                        Role = UserRole.Manager
                    }
                };

                var numOfUsers = Random.Shared.Next(minNumUsersPerProject, maxNumUsersPerProject + 1);
                var numOfManagers = Random.Shared.Next(minNumManagersPerProject, maxNumManagersPerProject + 1);

                var usersForProject = users.Where(u => u.Id != pOwner.Id).OrderBy(x => Guid.NewGuid()).Take(numOfUsers - 1).ToList();
                var managersForProject = usersForProject.Take(numOfManagers).ToList();

                foreach (var user in usersForProject)
                {
                    projectUsers.Add(new ProjectUser
                    {
                        Project = project,
                        User = user,
                        Role = managersForProject.Contains(user) ? UserRole.Manager : UserRole.Developer
                    });
                }
                context.ProjectUsers.AddRange(projectUsers);


                // taski

                var tasks = new List<Task>();
                var numOfTasks = Random.Shared.Next(minNumTasksPerProject, maxNumTasksPerProject + 1);

                for (int j = 0; j < numOfTasks; j++)
                {
                    var tVerb = Verbs[Random.Shared.Next(Verbs.Count)];
                    var tAdjective = Adjectives[Random.Shared.Next(Adjectives.Count)];
                    var tNoun = Nouns[Random.Shared.Next(Nouns.Count)];
                    tAdjective = tAdjective.OdmienPrzymiotnik(tNoun);
                    var tTitle = $"{tVerb} {tAdjective} {tNoun}";
                    var tDescription = LoremIpsum.Substring(0, Random.Shared.Next(50, LoremIpsum.Length));
                    var tType = (TaskType)Random.Shared.Next(Enum.GetValues(typeof(TaskType)).Length);
                    var tStatus = TaskStatus.Open;
                    var tPriority = (TaskPriority)Random.Shared.Next(Enum.GetValues(typeof(TaskPriority)).Length);
                    var tCreatedAt = pCreatedAt.AddDays(Random.Shared.Next(1, 3)).AddHours(Random.Shared.Next(1, 24));
                    var tUpdatedAt = (Random.Shared.NextDouble() < 0.8) ? tCreatedAt : tCreatedAt.AddDays(Random.Shared.Next(1, 10));   // 20% ze bedzie edytowany
                    var tReporter = managersForProject[Random.Shared.Next(managersForProject.Count)];

                    tasks.Add(new Task
                    {
                        Title = tTitle,
                        Description = tDescription,
                        Type = tType,
                        Status = tStatus,
                        Priority = tPriority,
                        CreatedAt = tCreatedAt,
                        UpdatedAt = tUpdatedAt,
                        Reporter = tReporter,
                        Project = project
                    });
                }
                context.Tasks.AddRange(tasks);
                

                // worklogi init
                
                var worklogs = new List<Worklog>();


                // sprinty

                var sprints = new List<Sprint>();
                var numOfSprints = Random.Shared.Next(minNumSprintsPerProject, maxNumSprintsPerProject + 1);

                var tasksPerSprint = numOfTasks / numOfSprints;
                var remainingTasks = new List<Task>(tasks);

                var firstSprintCreationDate = pCreatedAt.AddDays(Random.Shared.Next(3, 5));
                var firstSprintStartDate = firstSprintCreationDate.AddDays(Random.Shared.Next(1, 4));

                for (int j = 0; j < numOfSprints; j++)
                {
                    var sAdjective = Adjectives[Random.Shared.Next(Adjectives.Count)];
                    var sNoun = Nouns[Random.Shared.Next(Nouns.Count)];
                    sAdjective = sAdjective.OdmienPrzymiotnik(sNoun);
                    var sName = $"Sprint {j + 1}: {sAdjective} {sNoun}";
                    var sCreatedAt = firstSprintCreationDate.AddDays(j * sprintLength);
                    var sStartDate = firstSprintStartDate.AddDays(j * sprintLength);
                    var sEndDate = sStartDate.AddDays(sprintLength);
                    var sStatus = (sStartDate > DateTime.UtcNow) ? SprintStatus.Planned : ((sEndDate < DateTime.UtcNow) ? SprintStatus.Completed : SprintStatus.Active);
                    var sCreator = managersForProject[Random.Shared.Next(managersForProject.Count)];
                    var sprint = new Sprint
                    {
                        Name = sName,
                        StartDate = sStartDate,
                        EndDate = sEndDate,
                        Status = sStatus,
                        CreatedAt = sCreatedAt,
                        Creator = sCreator,
                        Project = project
                    };
                    sprints.Add(sprint);
                    context.Sprints.Add(sprint);


                    // sprint tasks

                    var sprintTasks = new List<SprintTask>();
                    var devUsers = projectUsers.Where(pu => pu.Role == UserRole.Developer).Select(pu => pu.User).ToList();

                    for (int k = 0; k < tasksPerSprint && remainingTasks.Count > 0; k++)
                    {
                        var stTask = remainingTasks[0];
                        var stStatus = stTask.Status;
                        var stAssignee = devUsers[Random.Shared.Next(devUsers.Count)];

                        var stAddedLate = (Random.Shared.NextDouble() < 0.2); // 20% ze scope creep
                        var stAddedAt = stAddedLate ? sStartDate.AddDays(Random.Shared.Next(1, 3)) : sCreatedAt;
                        var stAddedBy = managersForProject[Random.Shared.Next(managersForProject.Count)];

                        var stIsRemoved = (Random.Shared.NextDouble() < 0.1); // 10% szans ze deprecated
                        var stRemovedAt = stIsRemoved ? stAddedAt.AddDays(Random.Shared.Next(1, 3)) : (DateTime?)null;
                        var stRemovedBy = stIsRemoved ? managersForProject[Random.Shared.Next(managersForProject.Count)] : null;

                        if (stIsRemoved)
                        {
                            remainingTasks.Add(stTask);
                            stStatus = TaskStatus.Open;
                            remainingTasks.RemoveAt(0);
                        }
                        else
                        {
                            stStatus = sStatus switch
                            {
                                SprintStatus.Planned => TaskStatus.Open,
                                SprintStatus.Active => (TaskStatus)Random.Shared.Next(Enum.GetValues(typeof(TaskStatus)).Length),
                                SprintStatus.Completed => TaskStatus.Closed,
                                _ => stTask.Status
                            };
                            remainingTasks.RemoveAt(0);
                        }

                        if (sStatus != SprintStatus.Planned)
                        {
                            // worklogi pt. 2
                            var workLogsPerTask = Random.Shared.Next(1, 4);

                            for (int l = 0; l < workLogsPerTask; l++)
                            {
                                var wlUser = Random.Shared.NextDouble() < 0.8 ? stAssignee: managersForProject[Random.Shared.Next(managersForProject.Count)]; // 20% ze manager ktorys
                                var wlDescription = LoremIpsum.Substring(0, Random.Shared.Next(20, LoremIpsum.Length));
                                var wlStartTime = sCreatedAt.AddDays(Random.Shared.Next(1, 12)).AddHours(Random.Shared.Next(1, 24));
                                var wlTimeSpent = TimeSpan.FromHours(Random.Shared.Next(1, 8));
                                var wlLoggedAt = wlStartTime.Add(wlTimeSpent).AddHours(Random.Shared.Next(0, 3));

                                worklogs.Add(new Worklog
                                {
                                    Task = stTask,
                                    User = wlUser,
                                    Description = wlDescription,
                                    StartTime = wlStartTime,
                                    TimeSpent = wlTimeSpent,
                                    LoggedAt = wlLoggedAt
                                });
                            }
                            context.Worklogs.AddRange(worklogs);
                        }

                        sprintTasks.Add(new SprintTask
                        {
                            Sprint = sprint,
                            Task = stTask,
                            AddedAt = stAddedAt,
                            AddedBy = stAddedBy,
                            RemovedAt = stRemovedAt,
                            RemovedBy = stRemovedBy,
                            Status = stStatus,
                            Assignee = stAssignee
                        });

                        stTask.Status = stStatus;
                    }
                    context.SprintTasks.AddRange(sprintTasks);
                }
            }
            context.SaveChanges();
        }

        public static void Clear(AppDbContext context)
        {
            context.Database.EnsureDeleted();
        }

        private static string OdmienPrzymiotnik(this string adjective, string noun)
        {
            var adjLast = adjective.Last();
            var nounLast = noun.Last();

            bool isFeminine = nounLast == 'a';

            if (isFeminine)
                return adjective[..^1] + "a";

            bool isNeutralOrPlural = "oeęyićś".Contains(nounLast);

            if (!isNeutralOrPlural)
                return adjective;

            bool endsWithY = adjLast == 'y';

            string stem = endsWithY
                ? adjective[..^1]
                : adjective;

            return endsWithY ? stem + "e" : adjective + "e";
        }
    }
}