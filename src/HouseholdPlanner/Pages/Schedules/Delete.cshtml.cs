// File: src/HouseholdPlanner/Pages/Schedules/Delete.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Schedules
{
    public class DeleteModel(PlannerDbContext db) : PageModel
    {
        [BindProperty]
        public TaskSchedule Schedule { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, string? returnUrl = null)
        {
            var schedule = await db.TaskSchedules
                .Include(s => s.PlannerTask)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            Schedule = schedule;
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? Url.Page("/Index", new { weekStart = GetMonday(schedule.Date).ToString("yyyy-MM-dd") })
                : returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var schedule = await db.TaskSchedules.FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null)
            {
                return NotFound();
            }

            var weekStart = GetMonday(schedule.Date);
            db.TaskSchedules.Remove(schedule);
            await db.SaveChangesAsync();

            var redirectUrl = !string.IsNullOrWhiteSpace(ReturnUrl)
                ? ReturnUrl
                : Url.Page("/Index", new { weekStart = weekStart.ToString("yyyy-MM-dd") });

            return Redirect(redirectUrl!);
        }

        private static DateOnly GetMonday(DateOnly date)
        {
            var delta = date.DayOfWeek == DayOfWeek.Sunday ? -6 : DayOfWeek.Monday - date.DayOfWeek;
            return date.AddDays(delta);
        }
    }
}