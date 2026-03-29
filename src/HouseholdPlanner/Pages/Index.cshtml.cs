// File: src/HouseholdPlanner/Pages/Index.cshtml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages
{
    public class IndexModel(PlannerDbContext db) : PageModel
    {
        // Week range
        public DateOnly WeekStart { get; private set; }
        public DateOnly WeekEnd { get; private set; }

        // Calendar structure
        public List<DateOnly> Days { get; private set; } = [];
        public List<TimeOnly> HourSlots { get; private set; } = [];

        // Task schedule blocks rendered on the grid
        public List<TaskScheduleBlockViewModel> ScheduleBlocks { get; private set; } = [];

        // Weekly task summary lists for the sidebar
        public List<PlannerTask> OverdueTasks { get; private set; } = [];
        public List<PlannerTask> DueThisWeekTasks { get; private set; } = [];
        public List<PlannerTask> UnassignedTasks { get; private set; } = [];

        public sealed class TaskScheduleBlockViewModel
        {
            public int Id { get; init; }
            public DateOnly Date { get; init; }
            public TimeOnly StartLocalTime { get; init; }
            public TimeSpan Duration { get; init; }
            public string TaskName { get; init; } = string.Empty;
            public string? AssigneeName { get; init; }
            public string? AssigneeColor { get; init; }

            /// <summary>
            /// True when this block overlaps (time-wise) with at least one
            /// other block for the same user on the same day.
            /// </summary>
            public bool IsDoubleBooked { get; init; }

            /// <summary>
            /// Index of the 30-minute grid row from 06:00 (0-based).
            /// </summary>
            public int StartHalfHourIndex { get; init; }

            /// <summary>
            /// How many 30-minute rows the block spans.
            /// </summary>
            public int HalfHourSpan { get; init; }
        }

        public async Task OnGetAsync(DateOnly? weekStart)
        {
            // Determine week start (Monday-based)
            WeekStart = weekStart ?? GetCurrentWeekMonday();
            WeekEnd = WeekStart.AddDays(6);

            // Build day list (Mon–Sun)
            Days = [.. Enumerable.Range(0, 7).Select(offset => WeekStart.AddDays(offset))];

            // Hour slots for the left-hand time column (06:00–21:00 / 22-ish)
            HourSlots = [.. Enumerable.Range(6, 16) // 6..21 -> 16 hours
                .Select(h => new TimeOnly(h, 0))];

            // Load schedules for this week
            var schedules = await db.TaskSchedules
                .Include(s => s.PlannerTask)
                    .ThenInclude(t => t.Assignee)
                .Where(s => s.Date >= WeekStart && s.Date <= WeekEnd)
                .ToListAsync();

            ScheduleBlocks = BuildScheduleBlocks(schedules);

            // Load tasks for sidebar summary
            await LoadWeeklyTaskSummaryAsync();
        }

        private async Task LoadWeeklyTaskSummaryAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Base query; adjust as needed if you later add soft-delete or completion flags.
            var tasksQuery = db.PlannerTasks
                .Include(t => t.Assignee)
                .AsQueryable();

            // Overdue: deadline before today
            OverdueTasks = await tasksQuery
                .Where(t => t.Deadline != null && t.Deadline < today)
                .OrderBy(t => t.Deadline)
                .ThenBy(t => t.Name)
                .Take(50)
                .ToListAsync();

            // Due this week: deadline within [WeekStart, WeekEnd]
            DueThisWeekTasks = await tasksQuery
                .Where(t => t.Deadline != null &&
                            t.Deadline >= WeekStart &&
                            t.Deadline <= WeekEnd)
                .OrderBy(t => t.Deadline)
                .ThenBy(t => t.Name)
                .Take(50)
                .ToListAsync();

            // Unassigned: no assignee set
            UnassignedTasks = await tasksQuery
                .Where(t => t.AssigneeId == null)
                .OrderBy(t => t.Deadline ?? WeekEnd.AddDays(7)) // nulls at end
                .ThenBy(t => t.Name)
                .Take(50)
                .ToListAsync();
        }

        private List<TaskScheduleBlockViewModel> BuildScheduleBlocks(List<TaskSchedule> schedules)
        {
            var result = new List<TaskScheduleBlockViewModel>();

            if (!schedules.Any())
            {
                return result;
            }

            // Group by day and user to detect overlaps per user/day
            var groupedByUserAndDay = schedules
                .GroupBy(s => new
                {
                    s.Date,
                    UserId = s.PlannerTask.AssigneeId
                });

            foreach (var group in groupedByUserAndDay)
            {
                var ordered = group
                    .OrderBy(s => s.StartLocalTime)
                    .ThenBy(s => s.Id)
                    .ToList();

                for (var i = 0; i < ordered.Count; i++)
                {
                    var schedule = ordered[i];
                    var start = schedule.StartLocalTime;
                    var duration = schedule.AmountOfTime;

                    if (duration <= TimeSpan.Zero)
                    {
                        duration = TimeSpan.FromMinutes(60);
                    }

                    var end = start.Add(duration);

                    // Overlap: any other block that intersects this one's [start, end)
                    var hasOverlap = ordered.Any(other =>
                        other.Id != schedule.Id &&
                        other.StartLocalTime < end &&
                        other.StartLocalTime.Add(other.AmountOfTime) > start);

                    // 06:00 => index 0, each index = 30 minutes
                    var startMinutesFromSix = (start.Hour - 6) * 60 + start.Minute;
                    var halfHourIndex = startMinutesFromSix / 30;

                    if (halfHourIndex < 0)
                    {
                        halfHourIndex = 0;
                    }
                    else if (halfHourIndex > 31)
                    {
                        halfHourIndex = 31;
                    }

                    var span = Math.Max(
                        1,
                        (int)Math.Round(duration.TotalMinutes / 30.0, MidpointRounding.AwayFromZero));

                    // Clamp span so it doesn't blow past the 32 rows
                    if (halfHourIndex + span > 32)
                    {
                        span = 32 - halfHourIndex;
                    }

                    result.Add(new TaskScheduleBlockViewModel
                    {
                        Id = schedule.Id,
                        Date = schedule.Date,
                        StartLocalTime = start,
                        Duration = duration,
                        TaskName = schedule.PlannerTask.Name,
                        AssigneeName = schedule.PlannerTask.Assignee?.Name,
                        AssigneeColor = schedule.PlannerTask.Assignee?.Color,
                        IsDoubleBooked = hasOverlap,
                        StartHalfHourIndex = halfHourIndex,
                        HalfHourSpan = span
                    });
                }
            }

            return result;
        }

        private static DateOnly GetCurrentWeekMonday()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var delta = today.DayOfWeek == DayOfWeek.Sunday
                ? -6
                : DayOfWeek.Monday - today.DayOfWeek;

            return today.AddDays(delta);
        }
    }
}
