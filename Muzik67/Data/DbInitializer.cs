using Microsoft.EntityFrameworkCore;
using Muzik67.Helpers;
using Muzik67.Models;

namespace Muzik67.Data
{
    public static class DbInitializer
    {
        public static void Initialize(MuzikDbContext context)
        {
            // Creates the database and all tables if they do not exist
            context.Database.EnsureCreated();

            EnsureStatusColumn(context);
            EnsureProposalColumns(context);
            CreateProgressTables(context);

            // 1. Seed Users
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        Email = "admin@muzik67.com",
                        PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                        Role = "Admin",
                        RegistrationDate = DateTime.UtcNow,
                        AccountStatus = "Active",
                        AvatarKey = "piano",
                        PinHash = PasswordHelper.HashPassword("1234")
                    },
                    new User
                    {
                        Username = "student1",
                        Email = "student1@muzik67.com",
                        PasswordHash = PasswordHelper.HashPassword("Student@123"),
                        Role = "Member",
                        RegistrationDate = DateTime.UtcNow,
                        AccountStatus = "Active",
                        AvatarKey = "star",
                        PinHash = PasswordHelper.HashPassword("2468")
                    }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // 2. Seed SubscriptionPlans
            if (!context.SubscriptionPlans.Any())
            {
                var plans = new List<SubscriptionPlan>
                {
                    new SubscriptionPlan
                    {
                        PlanName = "Free Explorer",
                        Price = 0.00m,
                        Duration = "Monthly",
                        Description = "Free access to basic musical notes, intro quizzes, and mini virtual piano.",
                        Status = "Active"
                    },
                    new SubscriptionPlan
                    {
                        PlanName = "Junior Maestro (Monthly)",
                        Price = 9.99m,
                        Duration = "Monthly",
                        Description = "Full access to all interactive music games, ear-training levels, and progress tracking.",
                        Status = "Active"
                    },
                    new SubscriptionPlan
                    {
                        PlanName = "Junior Maestro (Yearly)",
                        Price = 89.99m,
                        Duration = "Yearly",
                        Description = "Best value annual pass with unlimited songs, advanced rhythm modules, and certificates.",
                        Status = "Active"
                    }
                };
                context.SubscriptionPlans.AddRange(plans);
                context.SaveChanges();
            }

            // 3. Seed LearningContent
            if (!context.LearningContents.Any())
            {
                var contents = new List<LearningContent>
                {
                    new LearningContent
                    {
                        Title = "Introduction to the Treble Clef & Musical Notes",
                        Description = "Discover the 7 basic notes: C (Do), D (Re), E (Mi), F (Fa), G (Sol), A (La), B (Ti).",
                        BodyContent = "Listen to every note from C to B. Notice how each pitch rises. Then play the scale and say each solfege name aloud.",
                        ContentType = "Theory",
                        DifficultyLevel = "Easy",
                        CreatedBy = "admin",
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow,
                        ApprovalStatus = "Approved",
                        ApprovedBy = "admin",
                        ApprovedDate = DateTime.UtcNow,
                        SortOrder = 10
                    },
                    new LearningContent
                    {
                        Title = "Rhythm Fun: Quarter Notes and Half Notes",
                        Description = "Learn how beats work by clapping along with friendly musical rhythm cards.",
                        BodyContent = "Count a steady four-beat pulse. Tap once for a quarter note and hold for two counts when you see a half note.",
                        ContentType = "Theory",
                        DifficultyLevel = "Easy",
                        CreatedBy = "admin",
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow,
                        ApprovalStatus = "Approved",
                        ApprovedBy = "admin",
                        ApprovedDate = DateTime.UtcNow,
                        SortOrder = 20
                    },
                    new LearningContent
                    {
                        Title = "Meet the Keyboard: White Keys and Black Keys",
                        Description = "Understand how piano keys are arranged in repeating patterns of two and three black keys.",
                        BodyContent = "Find each group of two black keys. The white key immediately before that group is C. Use this pattern to find notes anywhere on a keyboard.",
                        ContentType = "Instrument",
                        DifficultyLevel = "Medium",
                        CreatedBy = "admin",
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow,
                        ApprovalStatus = "Approved",
                        ApprovedBy = "admin",
                        ApprovedDate = DateTime.UtcNow,
                        SortOrder = 30
                    }
                };
                context.LearningContents.AddRange(contents);
                context.SaveChanges();
            }

            // 4. Seed Games and Quizzes
            if (!context.Games.Any())
            {
                var game1 = new Game
                {
                    GameName = "Note Matcher",
                    Description = "Match letter names to the musical sounds played on the screen.",
                    DifficultyLevel = "Easy",
                    Status = "Active"
                };

                var game2 = new Game
                {
                    GameName = "Guess the Sound",
                    Description = "Listen carefully to instruments and identify if it's high, low, loud, or soft.",
                    DifficultyLevel = "Easy",
                    Status = "Active"
                };

                context.Games.AddRange(game1, game2);
                context.SaveChanges();

                // Add sample quizzes
                var quizzes = new List<GameQuiz>
                {
                    new GameQuiz
                    {
                        GameId = game1.GameId,
                        QuestionText = "What is the musical note that starts the C major scale?",
                        AnswerOptions = "C (Do), D (Re), G (Sol), B (Ti)",
                        CorrectAnswer = "C (Do)"
                    },
                    new GameQuiz
                    {
                        GameId = game1.GameId,
                        QuestionText = "Which note comes immediately after 'Fa' in the solfège scale?",
                        AnswerOptions = "Mi, Sol, La, Do",
                        CorrectAnswer = "Sol"
                    },
                    new GameQuiz
                    {
                        GameId = game2.GameId,
                        QuestionText = "A bird chirping produces what kind of pitch?",
                        AnswerOptions = "High Pitch, Low Pitch, Silent, None",
                        CorrectAnswer = "High Pitch"
                    }
                };
                context.GamesQuizzes.AddRange(quizzes);
                context.SaveChanges();
            }

            EnsureProposalActivities(context);
            EnsureDemoActivityData(context);
        }

        private static void EnsureDemoActivityData(MuzikDbContext context)
        {
            // SEED THE WHOLE PRESENTATION because empty dashboards have negative aura
            var now = DateTime.UtcNow;
            var demoLearner = context.Users.FirstOrDefault(user =>
                user.Username == "student1" && user.Role == "Member");
            if (demoLearner is null) return;

            var premiumPlan = context.SubscriptionPlans
                .Where(plan => plan.Price > 0 && plan.Status == "Active")
                .AsEnumerable()
                .OrderBy(plan => plan.Price)
                .FirstOrDefault();
            if (premiumPlan is not null && !context.UserSubscriptions.Any(subscription =>
                    subscription.UserId == demoLearner.UserId &&
                    subscription.Status == "Active" &&
                    subscription.EndDate > now))
            {
                context.UserSubscriptions.Add(new UserSubscription
                {
                    UserId = demoLearner.UserId,
                    PlanId = premiumPlan.PlanId,
                    StartDate = now.AddDays(-7),
                    EndDate = now.AddMonths(1),
                    Status = "Active"
                });
            }

            var demoLessons = context.LearningContents
                .Where(content => content.ApprovalStatus == "Approved" && content.Status == "Active")
                .OrderBy(content => content.SortOrder)
                .ThenBy(content => content.ContentId)
                .Take(2)
                .ToList();
            foreach (var lesson in demoLessons)
            {
                if (!context.LessonProgresses.Any(progress =>
                        progress.UserId == demoLearner.UserId && progress.ContentId == lesson.ContentId))
                {
                    context.LessonProgresses.Add(new LessonProgress
                    {
                        UserId = demoLearner.UserId,
                        ContentId = lesson.ContentId,
                        CompletedAt = now.AddDays(-3).AddMinutes(lesson.SortOrder)
                    });
                }
            }

            var demoGames = context.Games
                .Where(game => game.ApprovalStatus == "Approved" && game.Status == "Active")
                .OrderBy(game => game.SortOrder)
                .ThenBy(game => game.GameId)
                .Take(2)
                .ToList();
            foreach (var game in demoGames)
            {
                if (context.GameProgresses.Any(progress =>
                        progress.UserId == demoLearner.UserId && progress.GameId == game.GameId))
                {
                    continue;
                }

                var questionCount = context.GamesQuizzes.Count(question => question.GameId == game.GameId);
                if (questionCount == 0) continue;

                context.GameProgresses.Add(new GameProgress
                {
                    UserId = demoLearner.UserId,
                    GameId = game.GameId,
                    Attempts = 1,
                    Score = Math.Max(1, questionCount - 1),
                    Status = "Completed",
                    DateOfCompletion = now.AddDays(-2).AddMinutes(game.GameId)
                });
            }

            var demoBadges = new[]
            {
                new { Code = "first-lesson", Name = "First Steps" },
                new { Code = "sound-explorer", Name = "Sound Explorer" }
            };
            foreach (var demoBadge in demoBadges)
            {
                if (!context.UserBadges.Any(badge =>
                        badge.UserId == demoLearner.UserId && badge.BadgeCode == demoBadge.Code))
                {
                    context.UserBadges.Add(new UserBadge
                    {
                        UserId = demoLearner.UserId,
                        BadgeCode = demoBadge.Code,
                        BadgeName = demoBadge.Name,
                        EarnedAt = now.AddDays(-1)
                    });
                }
            }

            // Save once at the end so the mock data arrives as one happy little family
            context.SaveChanges();
        }

        private static void EnsureProposalActivities(MuzikDbContext context)
        {
            var now = DateTime.UtcNow;
            var adminName = context.Users.Where(user => user.Role == "Admin")
                .Select(user => user.Username).FirstOrDefault() ?? "admin";

            var demoLearner = context.Users.FirstOrDefault(user => user.Username == "student1" && user.Role == "Member");
            if (demoLearner is not null && string.IsNullOrWhiteSpace(demoLearner.PinHash))
            {
                demoLearner.AvatarKey = "star";
                demoLearner.PinHash = PasswordHelper.HashPassword("2468");
            }

            foreach (var content in context.LearningContents.Where(item => item.ApprovalStatus == ""))
            {
                content.ApprovalStatus = "Approved";
                content.ApprovedBy = adminName;
                content.ApprovedDate = now;
            }

            foreach (var game in context.Games.Where(item => item.ApprovalStatus == ""))
            {
                game.ApprovalStatus = "Approved";
                game.ApprovedBy = adminName;
                game.ApprovedDate = now;
            }

            var noteMatcher = context.Games.FirstOrDefault(game => game.GameName == "Note Matcher");
            if (noteMatcher is not null && !context.GamesQuizzes.Any(question =>
                    question.GameId == noteMatcher.GameId && question.ActivityType == "DragDrop"))
            {
                context.GamesQuizzes.Add(new GameQuiz
                {
                    GameId = noteMatcher.GameId,
                    QuestionText = "Drag the picture for middle C into the answer portal.",
                    AnswerOptions = "C,D,G,B",
                    CorrectAnswer = "C",
                    ActivityType = "DragDrop",
                    OptionImageUrls = "/images/note-c-card.svg,/images/note-d-card.svg,/images/note-g-card.svg,/images/note-b-card.svg",
                    SortOrder = 30
                });
            }

            var soundGame = context.Games.FirstOrDefault(game => game.GameName == "Guess the Sound");
            if (soundGame is not null)
            {
                foreach (var question in context.GamesQuizzes.Where(question => question.GameId == soundGame.GameId))
                {
                    question.ActivityType = "Sound";
                }
            }

            var melodyGame = context.Games.FirstOrDefault(game => game.GameName == "Melody Builder");
            if (melodyGame is null)
            {
                melodyGame = new Game
                {
                    GameName = "Melody Builder",
                    Description = "Listen to a short tune, then perform the same notes on the virtual keyboard.",
                    DifficultyLevel = "Medium",
                    Status = "Active",
                    ApprovalStatus = "Approved",
                    ApprovedBy = adminName,
                    ApprovedDate = now,
                    SortOrder = 30
                };
                context.Games.Add(melodyGame);
                context.SaveChanges();
            }

            if (!context.GamesQuizzes.Any(question => question.GameId == melodyGame.GameId))
            {
                context.GamesQuizzes.AddRange(
                    new GameQuiz
                    {
                        GameId = melodyGame.GameId,
                        QuestionText = "Listen, then play the four-note rising tune.",
                        AnswerOptions = "C,D,E,F,G,A,B",
                        CorrectAnswer = "C,D,E,G",
                        ActivityType = "Melody",
                        SortOrder = 10
                    },
                    new GameQuiz
                    {
                        GameId = melodyGame.GameId,
                        QuestionText = "Listen, then copy the musical answer.",
                        AnswerOptions = "C,D,E,F,G,A,B",
                        CorrectAnswer = "G,E,D,C",
                        ActivityType = "Melody",
                        SortOrder = 20
                    });
            }

            if (!context.LearningContents.Any(content => content.ContentType == "EarTraining"))
            {
                context.LearningContents.Add(new LearningContent
                {
                    Title = "Ear Training: High and Low Sounds",
                    Description = "Listen to two pitches and learn how to tell which sound is higher.",
                    BodyContent = "High sounds vibrate faster and feel lighter. Low sounds vibrate more slowly and feel deeper. Play the scale, compare the first and last notes, and describe what you hear.",
                    ContentType = "EarTraining",
                    DifficultyLevel = "Easy",
                    CreatedBy = adminName,
                    Status = "Active",
                    ApprovalStatus = "Approved",
                    ApprovedBy = adminName,
                    ApprovedDate = now,
                    SortOrder = 40,
                    CreatedDate = now
                });
            }

            context.SaveChanges();
        }

        private static void EnsureProposalColumns(MuzikDbContext context)
        {
            AddColumnIfMissing(context, "Users", "AvatarKey", "TEXT NOT NULL DEFAULT 'note'");
            AddColumnIfMissing(context, "Users", "PinHash", "TEXT NULL");

            AddColumnIfMissing(context, "LearningContent", "BodyContent", "TEXT NOT NULL DEFAULT ''");
            AddColumnIfMissing(context, "LearningContent", "MediaUrl", "TEXT NOT NULL DEFAULT ''");
            AddColumnIfMissing(context, "LearningContent", "SortOrder", "INTEGER NOT NULL DEFAULT 0");
            AddColumnIfMissing(context, "LearningContent", "ApprovalStatus", "TEXT NOT NULL DEFAULT 'Approved'");
            AddColumnIfMissing(context, "LearningContent", "ApprovedBy", "TEXT NULL");
            AddColumnIfMissing(context, "LearningContent", "ApprovedDate", "TEXT NULL");

            AddColumnIfMissing(context, "Games", "SortOrder", "INTEGER NOT NULL DEFAULT 0");
            AddColumnIfMissing(context, "Games", "ApprovalStatus", "TEXT NOT NULL DEFAULT 'Approved'");
            AddColumnIfMissing(context, "Games", "ApprovedBy", "TEXT NULL");
            AddColumnIfMissing(context, "Games", "ApprovedDate", "TEXT NULL");

            AddColumnIfMissing(context, "GamesQuiz", "ActivityType", "TEXT NOT NULL DEFAULT 'MultipleChoice'");
            AddColumnIfMissing(context, "GamesQuiz", "MediaUrl", "TEXT NOT NULL DEFAULT ''");
            AddColumnIfMissing(context, "GamesQuiz", "OptionImageUrls", "TEXT NOT NULL DEFAULT ''");
            AddColumnIfMissing(context, "GamesQuiz", "SortOrder", "INTEGER NOT NULL DEFAULT 0");
        }

        private static void AddColumnIfMissing(MuzikDbContext context, string table, string column, string definition)
        {
            var connection = context.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose) connection.Open();
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info(\"{table}\");";
                using var reader = command.ExecuteReader();
                var exists = false;
                while (reader.Read())
                {
                    if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                reader.Close();
                if (!exists)
                {
                    command.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition};";
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (shouldClose) connection.Close();
            }
        }

        private static void EnsureStatusColumn(MuzikDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
            {
                connection.Open();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA table_info('GameProgress');";
                using var reader = command.ExecuteReader();
                var hasStatus = false;
                while (reader.Read())
                {
                    if (string.Equals(reader.GetString(1), "Status", StringComparison.OrdinalIgnoreCase))
                    {
                        hasStatus = true;
                        break;
                    }
                }

                reader.Close();
                if (!hasStatus)
                {
                    command.CommandText = "ALTER TABLE \"GameProgress\" ADD COLUMN \"Status\" TEXT NOT NULL DEFAULT 'Completed';";
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (shouldClose)
                {
                    connection.Close();
                }
            }
        }

        private static void CreateProgressTables(MuzikDbContext context)
        {
            context.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS "LessonProgress" (
                    "LessonProgressId" INTEGER NOT NULL CONSTRAINT "PK_LessonProgress" PRIMARY KEY AUTOINCREMENT,
                    "UserId" INTEGER NOT NULL,
                    "ContentId" INTEGER NOT NULL,
                    "CompletedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_LessonProgress_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE,
                    CONSTRAINT "FK_LessonProgress_LearningContent_ContentId" FOREIGN KEY ("ContentId") REFERENCES "LearningContent" ("ContentId") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_LessonProgress_UserId_ContentId" ON "LessonProgress" ("UserId", "ContentId");
                CREATE INDEX IF NOT EXISTS "IX_LessonProgress_ContentId" ON "LessonProgress" ("ContentId");

                CREATE TABLE IF NOT EXISTS "UserBadges" (
                    "UserBadgeId" INTEGER NOT NULL CONSTRAINT "PK_UserBadges" PRIMARY KEY AUTOINCREMENT,
                    "UserId" INTEGER NOT NULL,
                    "BadgeCode" TEXT NOT NULL,
                    "BadgeName" TEXT NOT NULL,
                    "EarnedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_UserBadges_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserBadges_UserId_BadgeCode" ON "UserBadges" ("UserId", "BadgeCode");
                """);
        }
    }
}
