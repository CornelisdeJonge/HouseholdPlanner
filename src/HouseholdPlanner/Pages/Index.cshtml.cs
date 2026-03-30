// File: src/HouseholdPlanner/Pages/Index.cshtml.cs
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

        // User-specific view data
        public int? SelectedUserId { get; private set; }
        public PlannerUser? SelectedUser { get; private set; }
        public List<PlannerUser> AllUsers { get; private set; } = [];

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

            public int StartHalfHourIndex { get; init; }
            public int HalfHourSpan { get; init; }
        }

        public async Task OnGetAsync(DateOnly? weekStart, int? userId)
        {
            // Selected user
            SelectedUserId = userId;

            // Load all users (for chips/dropdown)
            AllUsers = await db.Users
                .OrderBy(u => u.SortOrder ?? int.MaxValue)
                .ThenBy(u => u.Name)
                .ToListAsync();

            // If a user is selected, load specific user
            if (SelectedUserId != null)
            {
                SelectedUser = AllUsers.FirstOrDefault(u => u.Id == SelectedUserId);
            }

            // Determine week boundaries
            WeekStart = weekStart ?? GetCurrentWeekMonday();
            WeekEnd = WeekStart.AddDays(6);

            // Build Mon–Sun day list
            Days = [.. Enumerable.Range(0, 7).Select(offset => WeekStart.AddDays(offset))];

            // Hour slots 06:00–21:00
            HourSlots = [.. Enumerable.Range(6, 16).Select(h => new TimeOnly(h, 0))];

            // Load all schedules for the week
            var schedules = await db.TaskSchedules
                .Include(s => s.PlannerTask)
                    .ThenInclude(t => t.Assignee)
                .Where(s => s.Date >= WeekStart && s.Date <= WeekEnd)
                .ToListAsync();

            // Filter by user if selected
            if (SelectedUserId != null)
            {
                schedules = [.. schedules.Where(s => s.PlannerTask.AssigneeId == SelectedUserId)];
            }

            // Build block VMs
            ScheduleBlocks = BuildScheduleBlocks(schedules);

            // Load sidebar task lists
            await LoadWeeklyTaskSummaryAsync();
        }

        private async Task LoadWeeklyTaskSummaryAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Base query
            var tasksQuery = db.PlannerTasks
                .Include(t => t.Assignee)
                .AsQueryable();

            // Filter overdue
            OverdueTasks = await tasksQuery
                .Where(t => t.Deadline != null && t.Deadline < today)
                .OrderBy(t => t.Deadline)
                .ThenBy(t => t.Name)
                .Take(50)
                .ToListAsync();

            // Filter due this week
            DueThisWeekTasks = await tasksQuery
                .Where(t =>
                    t.Deadline != null &&
                    t.Deadline >= WeekStart &&
                    t.Deadline <= WeekEnd)
                .OrderBy(t => t.Deadline)
                .ThenBy(t => t.Name)
                .Take(50)
                .ToListAsync();

            // Unassigned (these remain global)
            UnassignedTasks = await tasksQuery
                .Where(t => t.AssigneeId == null)
                .OrderBy(t => t.Deadline ?? WeekEnd.AddDays(7))
                .ThenBy(t => t.Name)
                .Take(50)
                .ToListAsync();

            // If viewer-specific, apply user filter to overdue + due-this-week
            if (SelectedUserId != null)
            {
                OverdueTasks = [.. OverdueTasks.Where(t => t.AssigneeId == SelectedUserId)];
                DueThisWeekTasks = [.. DueThisWeekTasks.Where(t => t.AssigneeId == SelectedUserId)];
            }
        }

        private List<TaskScheduleBlockViewModel> BuildScheduleBlocks(List<TaskSchedule> schedules)
        {
            var result = new List<TaskScheduleBlockViewModel>();
            if (!schedules.Any()) return result;

            var groupedByUserAndDay = schedules
                .GroupBy(s => new { s.Date, UserId = s.PlannerTask.AssigneeId });

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

                    // Overlap detection
                    var hasOverlap = ordered.Any(other =>
                        other.Id != schedule.Id &&
                        other.StartLocalTime < end &&
                        other.StartLocalTime.Add(other.AmountOfTime) > start);

                    // Compute half-hour slot index
                    var startMinutesFromSix = (start.Hour - 6) * 60 + start.Minute;
                    var halfHourIndex = startMinutesFromSix / 30;
                    halfHourIndex = Math.Clamp(halfHourIndex, 0, 31);

                    // Duration in half-hours
                    var span = Math.Max(1,
                        (int)Math.Round(duration.TotalMinutes / 30.0, MidpointRounding.AwayFromZero));

                    // Clamp to end of grid
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