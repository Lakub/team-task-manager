using System;
using TeamTaskManager.Models;
using TeamTaskManager.Models.Entities;
using TeamTaskManager.Models.Enums;
using User = TeamTaskManager.Models.Entities.User;
using Task = TeamTaskManager.Models.Entities.Task;
using TaskStatus = TeamTaskManager.Models.Enums.TaskStatus;
using TeamTaskManager.Services;
using System.Windows;

namespace TeamTaskManager.Helpers
{
    public static class SeedData
    {
        static readonly List<string> FirstNameOptions = new List<string>
        {
            "Aldona", "Teodor", "Rudolf", "Róża", "Hanna", "Zbigniew", "Bożena", "Barbara", "Władysława", "Filip", "Eryk", "Leon", "Mikołaj", "Dominik", "Bartosz", "Stefan", "Alicja", "Fryderyk", "Borys", "Lena", "Milena", "Róża", "Eugenia", "Wiesław", "Daria", "Wiktoria", "Irena", "Tomasz", "Romuald", "Aleksander", "Zuzanna", "Daria", "Dominika", "Róża", "Klara", "Małgorzata", "Nataliia", "Genowefa", "Bernard", "Sylwester"
        };

        static readonly List<string> LastNameOptions = new List<string>
        {
            "Rudowicz", "Stępak", "Łoziński", "Włodarz", "Frelek", "Zavhorodnia", "Prokopchuk", "Radwański", "Środa", "Zając", "Małkiewicz", "Zębik", "Przerwa", "Olko", "Lemieszek", "Bronikowska", "Motyka", "Borcz", "Kubina", "Żuczek", "Ciochoń", "Powązka", "Dudczak", "Myszka", "Galas", "Kęska", "Talarowska", "Biegała", "Mytnik", "Chwalisz", "Hydzik", "Prądziński", "Tarnowska", "Szwejkowska", "Gaweł", "Pokojski", "Płoszaj", "Kruszczyński", "Poterek", "Kopel"
        };

        static readonly List<string> EmailDomainOptions = new List<string>
        {
            "gmail.com", "outlook.com", "proton.me", "hotmail.com", "wp.pl", "onet.pl", "interia.pl", "yahoo.com"
        };

        static readonly List<string> Adjectives = new List<string>
        {
            "przykładny", "roztoczowy", "kosmaty", "cętkowany", "marudny", "doroczny", "szefowski", "bilateralny", "niestanowczy", "hagiograficzny", "ważny", "stacyjny", "bezskuteczny", "areligijny", "cieniutki", "achromatyczny", "zasiadający", "lichwiarski", "postawny", "dzielnicowy", "fatalistyczny", "donośny", "buchasty", "starowierczy", "radioliniowy", "naftowy", "metrowy", "łukowy", "generacyjny", "szalbierczy", "żywy", "stalisty", "inżynieryjny", "klęskowy", "nieukojony", "wędrujący", "rozmoczony", "fonetyczny", "węglonośny", "kontuszowy", "uparty", "nieciągły", "piskliwy", "gruziński", "przepadły", "korbowy", "nieszczelny", "peremptoryczny", "dojrzały", "niegospodarny", "szosowy", "bezideowy", "zadziorny", "uliczny", "potajemny", "bombonierkowy", "mierzalny", "rajski", "letargiczny", "wpółprzytomny", "stanisławowski", "wielokształtny", "rdzeniowy", "elektrolityczny", "płochliwy", "leżący", "cyfrowy", "pełnomocny", "rozlewny", "piracki", "spirytusowy", "niezasadny", "członowy", "nienaruszalny", "niebujny", "niepohamowany", "przedmiejski", "rzeczownikowy", "efektowny", "miedziowy", "okazjonalny", "radiacyjny", "niepilny", "niepalny", "pokazowy", "prawoskrętny", "zabłąkany", "wyboisty", "lwi", "niesamodzielny", "cielesny", "ósmy", "łapczywy", "sekretny", "składkowy", "niestateczny", "wątpiący", "pulchny", "rytmiczny", "dodatkowy"
        };

        static readonly List<string> Nouns = new List<string>
        {
            "tytuł", "dźwięk", "szybkość", "chrząszcz", "okładka", "opieka", "kawałek", "kurczak", "małpa", "pociąg", "mgła", "szansa", "ciągnięcie", "wiadro", "nożyczki", "włosy", "ślimak", "wiara", "paczka", "deszcz ze śniegiem", "brama", "pchnięcie", "cent", "głos", "robak", "kopia", "ptak", "błąd", "dostosowanie", "porozumienie", "niemowlę", "powietrze", "kabel", "korzeń", "piątka", "wujek", "sztuczka", "meduza", "zupa", "zwierzę domowe", "palec", "strach", "bicz", "oszust", "ciecz", "moc", "rzeka", "narodziny", "igła", "park", "przyjaciel", "banan", "przyjemność", "śmiech", "religia", "broda", "piasek", "torba", "koszykówka", "impuls", "wzgórze", "człowiek", "pomiar", "jednostka", "humor", "koralik", "mycie", "dach", "kieszeń", "dziura", "zoo", "papier", "chłopak", "budynek", "pszczoła", "baza", "ignam", "deska", "firma", "cynk", "miedź", "ślimak", "pocałunek", "palenie", "użycie", "liść", "dzwon", "ból", "sałata", "łyżwa", "ciało", "próba", "zebra", "jedwab", "marnotrawstwo", "prześcieradło", "zespół", "urodziny", "zamówienie", "przestępstwo"
        };

        static readonly List<string> Verbs = new List<string>
        {
            "bredzić", "wzmagać", "czepiać", "budżetować", "dręczyć", "fiksować", "meblować", "honorować", "chromować", "stawać", "skaleczyć", "hołdować", "aportować", "salwować", "czytać", "bzykać", "konfigurować", "teleportować", "dumać", "kosztować", "ubolewać", "barwić", "dorwać", "demoralizować", "chlubić", "cmokać", "alokować", "degustować", "dziadować", "bajdurzyć", "bryknąć", "chałturzyć", "biwakować", "akompaniować", "dryfować", "dopompować", "grodzić", "kochać", "obalać", "dokrajać", "doprowadzić", "stykać", "zwiedzać", "balować", "definiować", "dybać", "dodrukować", "badać", "zwilżać", "obfotografować", "cedzić", "harować", "bełkotać", "zeswatać", "boleć", "golnąć", "bąblować", "obadać", "ignorować", "dopowiadać", "restaurować", "pisać", "mówić", "robić", "jeść", "pić", "spać", "biegać", "chodzić", "myśleć", "widzieć", "słyszeć", "czuć", "tworzyć", "niszczyć", "kupować", "sprzedawać", "otwierać", "zamykać", "zaczynać", "kończyć", "uczyć", "zapominać", "pamiętać", "szukać", "znajdować", "tracić", "zyskiwać", "budować", "burzyć", "prowadzić", "zatrzymywać", "wysyłać", "odbierać", "tłumaczyć", "sprawdzać", "liczyć", "mierzyć", "ważyć", "drukować"
        };

        static readonly string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";


        public static bool IsSeeded(AppDbContext context)
        {
            return context.UserAuths.Any();
        }

        public static void RandomSeed(
            AppDbContext context,
            IAuthService authService,

            // uzytkownicy
            int numUsers = 20,
            int numProjectOwners = 2,
            int minNumUsersPerProject = 5,
            int maxNumUsersPerProject = 9,                  

            // projekty
            int numProjects = 3,
            int minNumManagersPerProject = 1,
            int maxNumManagersPerProject = 2,

            // sprinty
            int minNumSprintsPerProject = 3,
            int maxNumSprintsPerProject = 7,
            int sprintLengthInDays = 14,

            // taski
            int minNumTasksPerSprint = 5,
            int maxNumTasksPerSprint = 10,
            float taskEditProbability = 0.2f,
            float taskScopeCreepProbability = 0.2f,
            float taskDeprecationProbability = 0.1f,
            
            // komentarze
            int minNumCommentsPerTask = 0,
            int maxNumCommentsPerTask = 4,
            int maxNumRepliesPerComment = 3,
            int maxCommentDepth = 3,
            float commentReplyProbability = 0.5f,

            // worklogi
            float worklogByManagerProbability = 0.2f)
        {
            context.Database.EnsureCreated();

            if (IsSeeded(context))
            {
                MessageBox.Show("Baza danych jest juz zainicjalizowana. Jesli chcesz zainicjowac od nowa, zrob najpierw cleardb.", "Inicjalizacja bazy danych", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // uzytkownicy

            var users = new List<User>();

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
                    LastLogin = LastLogin,
                    OrgRole = i < numProjectOwners ? OrgRole.Admin: OrgRole.User
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
                var pNounNoDiacritics = pNoun.RemoveDiacritics();
                var pAdjectiveNoDiacritics = pAdjective.RemoveDiacritics();
                var pKey = $"{pNounNoDiacritics[0..1]}{pAdjectiveNoDiacritics[0..1]}{Random.Shared.Next(100, 999)}".ToUpper();
                var project = new Project
                {
                    Name = pName,
                    Description = pDescription,
                    Key = pKey,
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

                // sprint count tutaj aby dalo sie obliczyc laczna ilosc taskow
                var numOfSprints = Random.Shared.Next(minNumSprintsPerProject, maxNumSprintsPerProject + 1);
                var sprintTaskCounts = new List<int>();
                var numOfTasks = 0;
                for (int j = 0; j < numOfSprints; j++)
                {
                    var tasksForSprint = Random.Shared.Next(minNumTasksPerSprint, maxNumTasksPerSprint + 1);
                    sprintTaskCounts.Add(tasksForSprint);
                    numOfTasks += tasksForSprint;
                }


                // taski

                var tasks = new List<Task>();

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
                    var tWasEdited = Random.Shared.NextDouble() < taskEditProbability;
                    var tUpdatedAt = tWasEdited ? tCreatedAt.AddDays(Random.Shared.Next(1, 10)) : tCreatedAt;
                    var tReporter = managersForProject[Random.Shared.Next(managersForProject.Count)];
                    var tPerProjectId = j;

                    var task = new Task
                    {
                        Title = tTitle,
                        Description = tDescription,
                        PerProjectId = tPerProjectId,
                        Type = tType,
                        Status = tStatus,
                        Priority = tPriority,
                        CreatedAt = tCreatedAt,
                        UpdatedAt = tUpdatedAt,
                        Reporter = tReporter,
                        Project = project
                    };

                    tasks.Add(task);
                    context.Tasks.Add(task);


                    // komentarze
                    var comments = new List<Comment>();
                    var numOfComments = Random.Shared.Next(minNumCommentsPerTask, maxNumCommentsPerTask + 1);

                    for (int k = 0; k < numOfComments; k++)
                    {
                        var cText = LoremIpsum.Substring(0, Random.Shared.Next(20, LoremIpsum.Length));
                        var cCommenter = projectUsers[Random.Shared.Next(projectUsers.Count)].User;
                        var cCreatedAt = tCreatedAt.AddDays(Random.Shared.Next(0, 10)).AddHours(Random.Shared.Next(1, 24));

                        var parentComment = new Comment
                        {
                            Task = task,
                            Commenter = cCommenter,
                            Text = cText,
                            CreatedAt = cCreatedAt
                        };

                        comments.Add(parentComment);

                        var replies = new List<Comment>();

                        replies.AddRange(GenerateReplies(task, parentComment, projectUsers, maxNumRepliesPerComment, commentReplyProbability, 0, maxCommentDepth));
                    }
                    context.Comments.AddRange(comments);
                }


                // worklogi init
                
                var worklogs = new List<Worklog>();


                // sprinty

                var sprints = new List<Sprint>();

                var remainingTasks = new List<Task>(tasks);

                var firstSprintCreationDate = pCreatedAt.AddDays(Random.Shared.Next(3, 5));
                var firstSprintStartDate = firstSprintCreationDate.AddDays(Random.Shared.Next(1, 4));

                for (int j = 0; j < numOfSprints; j++)
                {
                    var sAdjective = Adjectives[Random.Shared.Next(Adjectives.Count)];
                    var sNoun = Nouns[Random.Shared.Next(Nouns.Count)];
                    sAdjective = sAdjective.OdmienPrzymiotnik(sNoun);
                    var sName = $"Sprint {j + 1}: {sAdjective} {sNoun}";
                    var sCreatedAt = firstSprintCreationDate.AddDays(j * sprintLengthInDays);
                    var sStartDate = firstSprintStartDate.AddDays(j * sprintLengthInDays);
                    var sEndDate = sStartDate.AddDays(sprintLengthInDays);
                    var sStatus = (sStartDate > DateTime.UtcNow)
                                  ? SprintStatus.Planned : ((sEndDate < DateTime.UtcNow)
                                  ? SprintStatus.Completed : SprintStatus.Active);
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

                    var sTaskCount = sprintTaskCounts[j];
                    var sprintTasks = new List<SprintTask>();
                    var devUsers = projectUsers.Where(pu => pu.Role == UserRole.Developer).Select(pu => pu.User).ToList();

                    for (int k = 0; k < sTaskCount; k++)
                    {
                        var stTask = remainingTasks[0];
                        var stStatus = stTask.Status;
                        var stAssignee = devUsers[Random.Shared.Next(devUsers.Count)];

                        var stAddedLate = sStatus != SprintStatus.Planned && (Random.Shared.NextDouble() < taskScopeCreepProbability);
                        var stAddedAt = stAddedLate ? sStartDate.AddDays(Random.Shared.Next(1, 3)) : sCreatedAt;
                        var stAddedBy = managersForProject[Random.Shared.Next(managersForProject.Count)];

                        var stIsRemoved = sStatus != SprintStatus.Planned && (Random.Shared.NextDouble() < taskDeprecationProbability);
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
                                var wlByManager = Random.Shared.NextDouble() < worklogByManagerProbability;
                                var wlUser = wlByManager ? managersForProject[Random.Shared.Next(managersForProject.Count)] : stAssignee;
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
                        stTask.Assignee = stAssignee;
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

        static private List<Comment> GenerateReplies(
            Task task, Comment parentComment, List<ProjectUser> projectUsers,
            int maxNumOfReplies = 3, float replyProbability = 0.5f,
            int currentDepth = 0, int maxDepth = 3)
        {
            var comments = new List<Comment>();

            if (currentDepth >= maxDepth)
                return comments;

            var hasReplies = Random.Shared.NextDouble() < replyProbability;
            if (!hasReplies)
                return comments;

            var numOfReplies = Random.Shared.Next(1, maxNumOfReplies + 1);

            for (int i = 0; i < numOfReplies; i++)
            {
                var rText = LoremIpsum.Substring(0, Random.Shared.Next(20, LoremIpsum.Length));

                var rCommenter = projectUsers[Random.Shared.Next(projectUsers.Count)].User;

                var rCreatedAt = parentComment.CreatedAt.AddHours(Random.Shared.Next(1, 24));

                var reply = new Comment
                {
                    Task = task,
                    Commenter = rCommenter,
                    Text = rText,
                    CreatedAt = rCreatedAt,
                    ParentComment = parentComment
                };

                comments.Add(reply);

                comments.AddRange(GenerateReplies(task, reply, projectUsers, maxNumOfReplies, replyProbability, currentDepth + 1, maxDepth));
            }

            return comments;
        }
    }
}
