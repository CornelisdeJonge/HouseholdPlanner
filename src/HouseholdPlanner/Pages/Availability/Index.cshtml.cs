// File: src/HouseholdPlanner/Pages/Availability/Index.cshtml.cs
using System.ComponentModel.DataAnnotations;
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Availability
{
    public class IndexModel(PlannerDbContext db) : PageModel
    {
        public IList<SlotRow> Slots { get; private set; } = [];

        public IList<SelectListItem> UserOptions { get; private set; } = [];

        public IList<SelectListItem> DayOfWeekOptions { get; private set; } = [];

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            if (Input.EndLocalTime <= Input.StartLocalTime)
            {
                ModelState.AddModelError(nameof(Input.EndLocalTime), "End time must be after start time.");
                await LoadAsync();
                return Page();
            }

            var user = await db.Users.FindAsync(Input.UserId);
            if (user == null)
            {
                ModelState.AddModelError(nameof(Input.UserId), "Selected user does not exist.");
                await LoadAsync();
                return Page();
            }

            var slot = new AvailabilitySlot
            {
                User = user,
                DayOfWeek = Input.DayOfWeek,
                StartLocalTime = Input.StartLocalTime,
                EndLocalTime = Input.EndLocalTime
            };

            db.AvailabilitySlots.Add(slot);
            await db.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var slot = await db.AvailabilitySlots.FindAsync(id);
            if (slot != null)
            {
                db.AvailabilitySlots.Remove(slot);
                await db.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        private async Task LoadAsync()
        {
            UserOptions = await db.Users
                .OrderBy(u => u.Name)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                })
                .ToListAsync();

            DayOfWeekOptions = BuildDayOfWeekOptions();

            Slots = await db.AvailabilitySlots
                .Include(a => a.User)
                .OrderBy(a => a.User.Name)
                .ThenBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartLocalTime)
                .Select(a => new SlotRow
                {
                    Id = a.Id,
                    UserName = a.User.Name,
                    DayOfWeek = a.DayOfWeek,
                    StartLocalTime = a.StartLocalTime,
                    EndLocalTime = a.EndLocalTime
                })
                .ToListAsync();
        }

        private static List<SelectListItem> BuildDayOfWeekOptions()
        {
            var orderedDays = new[]
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday
            };

            return [.. orderedDays.Select(static d => new SelectListItem
                {
                    Value = ((int)d).ToString(),
                    Text = d.ToString()
                })];
        }

        public class InputModel
        {
            [Required]
            [Display(Name = "Person")]
            public int UserId { get; set; }

            [Required]
            [Display(Name = "Day of week")]
            public DayOfWeek DayOfWeek { get; set; }

            [Required]
            [Display(Name = "Start time")]
            public TimeOnly StartLocalTime { get; set; }

            [Required]
            [Display(Name = "End time")]
            public TimeOnly EndLocalTime { get; set; }
        }

        public class SlotRow
        {
            public int Id { get; set; }

            public string UserName { get; set; } = string.Empty;

            public DayOfWeek DayOfWeek { get; set; }

            public TimeOnly StartLocalTime { get; set; }

            public TimeOnly EndLocalTime { get; set; }
        }
    }
}