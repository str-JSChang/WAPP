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
                        AccountStatus = "Active"
                    },
                    new User
                    {
                        Username = "student1",
                        Email = "student1@muzik67.com",
                        PasswordHash = PasswordHelper.HashPassword("Student@123"),
                        Role = "Member",
                        RegistrationDate = DateTime.UtcNow,
                        AccountStatus = "Active"
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
                        ContentType = "Theory",
                        DifficultyLevel = "Easy",
                        CreatedBy = "admin",
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow
                    },
                    new LearningContent
                    {
                        Title = "Rhythm Fun: Quarter Notes and Half Notes",
                        Description = "Learn how beats work by clapping along with friendly musical rhythm cards.",
                        ContentType = "Theory",
                        DifficultyLevel = "Easy",
                        CreatedBy = "admin",
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow
                    },
                    new LearningContent
                    {
                        Title = "Meet the Keyboard: White Keys and Black Keys",
                        Description = "Understand how piano keys are arranged in repeating patterns of two and three black keys.",
                        ContentType = "Instrument",
                        DifficultyLevel = "Medium",
                        CreatedBy = "admin",
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow
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
        }
    }
}
