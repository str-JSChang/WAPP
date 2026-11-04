using Microsoft.EntityFrameworkCore;
using Muzik67.Models;

namespace Muzik67.Data
{
    public class MuzikDbContext : DbContext
    {
        public MuzikDbContext(DbContextOptions<MuzikDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<LearningContent> LearningContents => Set<LearningContent>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<GameQuiz> GamesQuizzes => Set<GameQuiz>();
        public DbSet<GameProgress> GameProgresses => Set<GameProgress>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
        public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
        public DbSet<UserBadge> UserBadges => Set<UserBadge>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // 2. LearningContent
            modelBuilder.Entity<LearningContent>(entity =>
            {
                entity.ToTable("LearningContent");
                entity.HasKey(e => e.ContentId);
            });

            // 3. Games
            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Games");
                entity.HasKey(e => e.GameId);
            });

            // 4. GamesQuiz
            modelBuilder.Entity<GameQuiz>(entity =>
            {
                entity.ToTable("GamesQuiz");
                entity.HasKey(e => e.QuestionId);
                entity.HasOne(e => e.Game)
                      .WithMany(g => g.Quizzes)
                      .HasForeignKey(e => e.GameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 5. GameProgress
            modelBuilder.Entity<GameProgress>(entity =>
            {
                entity.ToTable("GameProgress");
                entity.HasKey(e => e.ProgressId);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Game)
                      .WithMany()
                      .HasForeignKey(e => e.GameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. SubscriptionPlans
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("SubscriptionPlans");
                entity.HasKey(e => e.PlanId);
            });

            // 7. UserSubscriptions
            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.ToTable("UserSubscriptions");
                entity.HasKey(e => e.SubscriptionId);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Plan)
                      .WithMany()
                      .HasForeignKey(e => e.PlanId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LessonProgress>(entity =>
            {
                entity.ToTable("LessonProgress");
                entity.HasKey(e => e.LessonProgressId);
                entity.HasIndex(e => new { e.UserId, e.ContentId }).IsUnique();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Content)
                      .WithMany()
                      .HasForeignKey(e => e.ContentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserBadge>(entity =>
            {
                entity.ToTable("UserBadges");
                entity.HasKey(e => e.UserBadgeId);
                entity.HasIndex(e => new { e.UserId, e.BadgeCode }).IsUnique();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
