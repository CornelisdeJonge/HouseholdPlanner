// File: src/HouseholdPlanner/Models/WeeklyTaskSummaryViewModel.cs
using System;
using System.Collections.Generic;

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
}