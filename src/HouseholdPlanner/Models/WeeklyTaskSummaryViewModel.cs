// File: src/HouseholdPlanner/Models/WeeklyTaskSummaryViewModel.cs
namespace HouseholdPlanner.Models
{
    public sealed class WeeklyTaskSummaryItem
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateOnly? Deadline { get; init; }
    }

    public sealed class WeeklyTaskSummaryViewModel
    {
        public DateOnly WeekStart { get; init; }

        public DateOnly WeekEnd { get; init; }

        public int OverdueCount { get; init; }

        public int DueThisWeekCount { get; init; }

        public int UnassignedCount { get; init; }

        public IReadOnlyList<WeeklyTaskSummaryItem> OverdueTasks { get; init; } =
            Array.Empty<WeeklyTaskSummaryItem>();

        public IReadOnlyList<WeeklyTaskSummaryItem> DueThisWeekTasks { get; init; } =
            Array.Empty<WeeklyTaskSummaryItem>();

        public IReadOnlyList<WeeklyTaskSummaryItem> UnassignedTasks { get; init; } =
            Array.Empty<WeeklyTaskSummaryItem>();
    }

    public sealed class TaskScheduleBlockViewModel
    {
        public int Id { get; init; }
        public DateOnly Date { get; init; }
        public TimeOnly StartLocalTime { get; init; }
        public TimeSpan Duration { get; init; }
        public string TaskName { get; init; } = string.Empty;
        public string? AssigneeName { get; init; }
        public string? AssigneeColor { get; init; }
        public bool IsDoubleBooked { get; init; }

        // For grid placement (06:00–22:00, 30-minute rows)
        public int StartHalfHourIndex { get; init; }
        public int HalfHourSpan { get; init; }
    }

}