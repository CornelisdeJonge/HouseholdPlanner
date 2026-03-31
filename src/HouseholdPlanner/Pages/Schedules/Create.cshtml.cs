// File: src/HouseholdPlanner/Pages/Schedules/Create.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using HouseholdPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Schedules
{
    public class CreateModel(PlannerDbContext db) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string TaskName { get; private set; } = string.Empty;
        public string? AssigneeDisplay { get; private set; }

        public async Task<IActionResult> OnGetAsync(
            int taskId,
            string? returnUrl = null,
            DateOnly? date = null,
            TimeOnly? start = null)
        {
            var task = await db.PlannerTasks
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            TaskName = task.Name;
            AssigneeDisplay = task.Assignee?.Name ?? "Unassigned";

            Input.TaskId = taskId;

            var localNow = DateTime.Now;
            Input.Date = date ?? DateOnly.FromDateTime(localNow);
            Input.StartLocalTime = start ?? SnapToQuarterHour(TimeOnly.FromDateTime(localNow));
            Input.DurationMinutes = 60;

            Input.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? Url.Page("/Index", new { weekStart = GetMonday(Input.Date).ToString("yyyy-MM-dd") })
                : returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadTaskMetaAsync();
                EnsureReturnUrl();
                return Page();
            }

            var task = await db.PlannerTasks
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == Input.TaskId);

            if (task == null)
            {
                return NotFound();
            }

            // IMPORTANT: Task must be assigned so we can set TaskSchedule.UserId (FK)
            if (task.AssigneeId == null)
            {
                ModelState.AddModelError(string.Empty,
                    "Assign this task to a person before scheduling time for it.");
                await LoadTaskMetaAsync();
                EnsureReturnUrl();
                return Page();
            }

            var snappedStart = SnapToQuarterHour(Input.StartLocalTime);
            var durationMinutes = NormalizeDuration(Input.DurationMinutes);
            var duration = TimeSpan.FromMinutes(durationMinutes);

            var schedule = new TaskSchedule
            {
                PlannerTaskId = task.Id,
                UserId = task.AssigneeId.Value, // <-- FIX: set FK to users
                Date = Input.Date,
                StartLocalTime = snappedStart,
                AmountOfTime = duration
            };

            db.TaskSchedules.Add(schedule);
            await db.SaveChangesAsync();

            if (task.Assignee != null &&
                IsOutsideAvailability(task.Assignee, Input.Date, snappedStart, duration))
            {
                TempData["OutsideAvailabilityWarning"] =
                    $"Planned block for {task.Assignee.Name} is outside their availability.";
            }

            var redirectUrl = !string.IsNullOrWhiteSpace(Input.ReturnUrl)
                ? Input.ReturnUrl
                : Url.Page("/Index", new { weekStart = GetMonday(Input.Date).ToString("yyyy-MM-dd") });

            return Redirect(redirectUrl!);
        }

        private async Task LoadTaskMetaAsync()
        {
            var task = await db.PlannerTasks
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == Input.TaskId);

            if (task != null)
            {
                TaskName = task.Name;
                AssigneeDisplay = task.Assignee?.Name ?? "Unassigned";
            }
        }

        private void EnsureReturnUrl()
        {
            if (string.IsNullOrWhiteSpace(Input.ReturnUrl))
            {
                Input.ReturnUrl = Url.Page("/Index",
                    new { weekStart = GetMonday(Input.Date).ToString("yyyy-MM-dd") });
            }
        }

        private static TimeOnly SnapToQuarterHour(TimeOnly time)
        {
            int totalMinutes = time.Hour * 60 + time.Minute;

            // Snap to nearest 15-minute block
            int snappedMinutes = (int)(Math.Round(totalMinutes / 15.0) * 15);

            // Handle overflow past 24:00 (optional: clamp instead)
            if (snappedMinutes >= 24 * 60)
                snappedMinutes = 24 * 60 - 1; // 23:59 or adjust to 23:45 if preferred

            int hour = snappedMinutes / 60;
            int minute = snappedMinutes % 60;

            return new TimeOnly(hour, minute);
        }
        private static int NormalizeDuration(int requestedMinutes)
        {
            if (requestedMinutes <= 0)
            {
                return 60;
            }

            var snapped = (int)Math.Round(requestedMinutes / 30.0, MidpointRounding.AwayFromZero) * 30;
            if (snapped < 30) snapped = 30;
            if (snapped > 8 * 60) snapped = 8 * 60;

            return snapped;
        }

        private static DateOnly GetMonday(DateOnly date)
        {
            var delta = date.DayOfWeek == DayOfWeek.Sunday
                ? -6
                : DayOfWeek.Monday - date.DayOfWeek;

            return date.AddDays(delta);
        }

        private bool IsOutsideAvailability(PlannerUser assignee, DateOnly date, TimeOnly start, TimeSpan duration)
        {
            var end = start.Add(duration);

            var slots = db.AvailabilitySlots
                .Where(s => s.UserId == assignee.Id && s.DayOfWeek == date.DayOfWeek)
                .ToList();

            if (!slots.Any())
            {
                return true;
            }

            return !slots.Any(slot =>
                slot.StartLocalTime <= start &&
                slot.EndLocalTime >= end);
        }
    }
}