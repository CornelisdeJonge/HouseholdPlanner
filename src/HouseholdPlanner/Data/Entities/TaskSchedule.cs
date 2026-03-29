// File: src/HouseholdPlanner/Data/Entities/TaskSchedule.cs
using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Data.Entities
{
    /// <summary>
    /// A scheduled occurrence of a PlannerTask for a specific user on a given date.
    /// Uses StartLocalTime + AmountOfTime instead of EndLocalTime.
    /// </summary>
    public class TaskSchedule
    {
        public int Id { get; set; }

        public int PlannerTaskId { get; set; }

        public PlannerTask PlannerTask { get; set; } = null!;

        public int UserId { get; set; }

        public PlannerUser User { get; set; } = null!;

        /// <summary>
        /// Local calendar date for the task occurrence (week starts Monday in UI logic).
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Local start time for this occurrence (no timezone conversion done here).
        /// </summary>
        [Required]
        public TimeOnly StartLocalTime { get; set; }

        /// <summary>
        /// Duration of the scheduled task.
        /// </summary>
        [Required]
        public TimeSpan AmountOfTime { get; set; }

        public bool IsCompleted { get; set; }
    }

}
