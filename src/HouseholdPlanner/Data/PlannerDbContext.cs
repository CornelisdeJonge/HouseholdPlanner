// File: src/HouseholdPlanner/Data/PlannerDbContext.cs
using HouseholdPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Data
{
    public class PlannerDbContext : DbContext
    {
        public PlannerDbContext(DbContextOptions<PlannerDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<PlannerTask> Tasks => Set<PlannerTask>();
        public DbSet<Subtask> Subtasks => Set<Subtask>();
        public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
        public DbSet<TaskSchedule> TaskSchedules => Set<TaskSchedule>();
        public DbSet<Meal> Meals => Set<Meal>();
        public DbSet<MealTag> MealTags => Set<MealTag>();
        public DbSet<MealTagMap> MealTagMaps => Set<MealTagMap>();
        public DbSet<MealPlan> MealPlans => Set<MealPlan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
                entity.Property(x => x.ColorHex).HasMaxLength(9);
                entity.Property(x => x.SortOrder);
            });

            modelBuilder.Entity<PlannerTask>(entity =>
            {
                entity.ToTable("tasks");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Description).HasMaxLength(2000);
                entity.Property(x => x.Deadline).HasColumnType("date");
            });

            modelBuilder.Entity<Subtask>(entity =>
            {
                entity.ToTable("subtasks");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.HasOne(x => x.PlannerTask)
                    .WithMany(t => t.Subtasks)
                    .HasForeignKey(x => x.PlannerTaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AvailabilitySlot>(entity =>
            {
                entity.ToTable("availability_slots");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.DayOfWeek)
                    .HasConversion<int>();
                entity.Property(x => x.StartLocalTime)
                    .HasColumnType("time");
                entity.Property(x => x.EndLocalTime)
                    .HasColumnType("time");
                entity.HasOne(x => x.User)
                    .WithMany(u => u.AvailabilitySlots)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TaskSchedule>(entity =>
            {
                entity.ToTable("task_schedules");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Date);
                entity.Property(x => x.StartLocalTime)
                    .HasColumnType("time");
                entity.Property(x => x.AmountOfTime)
                    .HasColumnType("interval");
                entity.HasOne(x => x.User)
                    .WithMany(u => u.TaskSchedules)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Meal>(entity =>
            {
                entity.ToTable("meals");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(x => x.Description)
                    .HasMaxLength(2000);
            });

            modelBuilder.Entity<MealTag>(entity =>
            {
                entity.ToTable("meal_tags");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(x => x.TagType)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<MealTagMap>(entity =>
            {
                entity.ToTable("meal_tag_maps");
                entity.HasKey(x => new { x.MealId, x.MealTagId });

                entity.HasOne(x => x.Meal)
                    .WithMany(m => m.MealTagMappings)
                    .HasForeignKey(x => x.MealId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.MealTag)
                    .WithMany(t => t.MealTagMappings)
                    .HasForeignKey(x => x.MealTagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MealPlan>(entity =>
            {
                entity.ToTable("meal_plans");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Date);
                entity.HasOne(x => x.Meal)
                    .WithMany(m => m.MealPlans)
                    .HasForeignKey(x => x.MealId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}