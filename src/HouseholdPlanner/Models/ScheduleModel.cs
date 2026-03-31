// File: src/HouseholdPlanner/Models/ScheduleModel.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Models
{
    public class ScheduleModel(PlannerDbContext db) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string TaskName { get; private set; } = string.Empty;
        public string? AssigneeDisplay { get; private set; }

        public bool IsEdit => Input.Id != 0;

        /// <summary>
        /// Handles both:
        /// - Edit: when id is provided
        /// - Create: when taskId is provided
        /// </summary>
        public async Task<IActionResult> OnGetAsync(
            int? id,
            int? taskId,
            string? returnUrl = null,
            DateOnly? date = null,
            TimeOnly? start = null)
        {
            if (id.HasValue)
            {
                // EDIT existing schedule
                var schedule = await db.TaskSchedules
                    .Include(s => s.PlannerTask)
                        .ThenInclude(t => t.Assignee)
                    .FirstOrDefaultAsync(s => s.Id == id.Value);

                if (schedule == null)
                {
                    return NotFound();
                }

                TaskName = schedule.PlannerTask.Name;
                AssigneeDisplay = schedule.PlannerTask.Assignee?.Name ?? "Unassigned";

                Input.Id = schedule.Id;
                Input.TaskId = schedule.PlannerTaskId;
                Input.Date = schedule.Date;
                Input.StartLocalTime = schedule.StartLocalTime;
                Input.DurationMinutes = (int)Math.Round(schedule.AmountOfTime.TotalMinutes);

                Input.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                    ? Url.Page("/Index", new { weekStart = GetMonday(schedule.Date).ToString("yyyy-MM-dd") })
                    : returnUrl;
            }
            else if (taskId.HasValue)
            {
                // CREATE new schedule for given task
                var task = await db.PlannerTasks
                    .Include(t => t.Assignee)
                    .FirstOrDefaultAsync(t => t.Id == taskId.Value);

                if (task == null)
                {
                    return NotFound();
                }

                TaskName = task.Name;
                AssigneeDisplay = task.Assignee?.Name ?? "Unassigned";

                Input.TaskId = task.Id;

                var localNow = DateTime.Now;
                Input.Date = date ?? DateOnly.FromDateTime(localNow);
                Input.StartLocalTime = start ?? SnapToQuarterHour(TimeOnly.FromDateTime(localNow));
                Input.DurationMinutes = 60;

                Input.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                    ? Url.Page("/Index", new { weekStart = GetMonday(Input.Date).ToString("yyyy-MM-dd") })
                    : returnUrl;
            }
            else
            {
                // Neither id nor taskId supplied → ambiguous request
                return BadRequest("Either a schedule id (id) or a task id (taskId) must be provided.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadMetaAsync();
                EnsureReturnUrl();
                return Page();
            }

            if (Input.Id == 0)
            {
                // CREATE
                var task = await db.PlannerTasks
                    .Include(t => t.Assignee)
                    .FirstOrDefaultAsync(t => t.Id == Input.TaskId);

                if (task == null)
                {
                    return NotFound();
                }

                // Task must be assigned so we can set TaskSchedule.UserId (FK)
                if (task.AssigneeId == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Assign this task to a person before scheduling time for it.");
                    await LoadMetaAsync();
                    EnsureReturnUrl();
                    return Page();
                }

                var snappedStart = SnapToQuarterHour(Input.StartLocalTime);
                var durationMinutes = NormalizeDuration(Input.DurationMinutes);
                var duration = TimeSpan.FromMinutes(durationMinutes);

                var schedule = new TaskSchedule
                {
                    PlannerTaskId = task.Id,
                    UserId = task.AssigneeId.Value,
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
            }
            else
            {
                // EDIT
                var schedule = await db.TaskSchedules
                    .Include(s => s.PlannerTask)
                        .ThenInclude(t => t.Assignee)
                    .FirstOrDefaultAsync(s => s.Id == Input.Id);

                if (schedule == null)
                {
                    return NotFound();
                }

                var snappedStart = SnapToQuarterHour(Input.StartLocalTime);
                var durationMinutes = NormalizeDuration(Input.DurationMinutes);
                var duration = TimeSpan.FromMinutes(durationMinutes);

                schedule.Date = Input.Date;
                schedule.StartLocalTime = snappedStart;
                schedule.AmountOfTime = duration;

                await db.SaveChangesAsync();

                if (schedule.PlannerTask.Assignee != null &&
                    IsOutsideAvailability(schedule.PlannerTask.Assignee,
                        schedule.Date,
                        snappedStart,
                        duration))
                {
                    TempData["OutsideAvailabilityWarning"] =
                        $"Planned block for {schedule.PlannerTask.Assignee.Name} is outside their availability.";
                }
            }

            var redirectUrl = !string.IsNullOrWhiteSpace(Input.ReturnUrl)
                ? Input.ReturnUrl
                : Url.Page("/Index", new { weekStart = GetMonday(Input.Date).ToString("yyyy-MM-dd") });

            return Redirect(redirectUrl!);
        }

        /// <summary>
        /// Load metadata for the current context (create vs edit)
        /// so the page re-renders correctly on validation errors.
        /// </summary>
        private async Task LoadMetaAsync()
        {
            if (Input.Id != 0)
            {
                // Edit context
                var schedule = await db.TaskSchedules
                    .Include(s => s.PlannerTask)
                        .ThenInclude(t => t.Assignee)
                    .FirstOrDefaultAsync(s => s.Id == Input.Id);

                if (schedule != null)
                {
                    TaskName = schedule.PlannerTask.Name;
                    AssigneeDisplay = schedule.PlannerTask.Assignee?.Name ?? "Unassigned";
                }
            }
            else
            {
                // Create context (we only know TaskId)
                var task = await db.PlannerTasks
                    .Include(t => t.Assignee)
                    .FirstOrDefaultAsync(t => t.Id == Input.TaskId);

                if (task != null)
                {
                    TaskName = task.Name;
                    AssigneeDisplay = task.Assignee?.Name ?? "Unassigned";
                }
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
                // Default duration
                return 60;
            }

            // Snap to nearest 15 minutes instead of 30
            var snapped = (int)Math.Round(requestedMinutes / 15.0, MidpointRounding.AwayFromZero) * 15;

            // Adjust min/max if you want to keep them
            if (snapped < 15) snapped = 15;
            if (snapped > 8 * 60) snapped = 8 * 60;

            return snapped;
        }
    }
}