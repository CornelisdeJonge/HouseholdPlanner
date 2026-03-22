// File: src/HouseholdPlanner/Data/Entities/PlannerTask.cs
// Adjust namespace/usings to match your existing project structure.
using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Data.Entities
{
    public enum TaskPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    public class PlannerTask
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public int? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Normal;

        /// <summary>
        /// Optional day the task should be done by (no specific time-of-day).
        /// </summary>
        public DateOnly? Deadline { get; set; }

        public ICollection<Subtask> Subtasks { get; set; } = [];
    }
}