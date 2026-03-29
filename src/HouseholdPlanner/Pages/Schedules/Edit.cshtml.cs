// File: src/HouseholdPlanner/Pages/Schedules/Edit.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using HouseholdPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Schedules
{
    public class EditModel(PlannerDbContext db) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string TaskName { get; private set; } = string.Empty;
        public string? AssigneeDisplay { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id, string? returnUrl = null)
        {
            var schedule = await db.TaskSchedules
                .Include(s => s.PlannerTask)
                    .ThenInclude(t => t.Assignee)
                .FirstOrDefaultAsync(s => s.Id == id);

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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadMetaAsync();
                // Ensure ReturnUrl remains non-null on validation errors
                if (string.IsNullOrWhiteSpace(Input.ReturnUrl))
                {
                    Input.ReturnUrl = Url.Page("/Index",
                        new { weekStart = GetMonday(Input.Date).ToString("yyyy-MM-dd") });
                }
                return Page();
            }

            var schedule = await db.TaskSchedules
                .Include(s => s.PlannerTask)
                    .ThenInclude(t => t.Assignee)
                .FirstOrDefaultAsync(s => s.Id == Input.Id);

            if (schedule == null)
            {
                return NotFound();
            }

            var snappedStart = SnapToHalfHour(Input.StartLocalTime);
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

            var redirectUrl = !string.IsNullOrWhiteSpace(Input.ReturnUrl)
                ? Input.ReturnUrl
                : Url.Page("/Index", new { weekStart = GetMonday(schedule.Date).ToString("yyyy-MM-dd") });

            return Redirect(redirectUrl!);
        }

        private async Task LoadMetaAsync()
        {
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

        private static TimeOnly SnapToHalfHour(TimeOnly time)
        {
            var minutes = time.Minute;
            var snappedMinutes = minutes < 15 ? 0 : minutes < 45 ? 30 : 0;
            var hour = time.Hour + (minutes >= 45 ? 1 : 0);

            if (hour >= 24)
            {
                hour = 23;
                snappedMinutes = 30;
            }

            return new TimeOnly(hour, snappedMinutes);
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
    }
}