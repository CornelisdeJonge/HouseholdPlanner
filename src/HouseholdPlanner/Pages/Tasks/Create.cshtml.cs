// File: src/HouseholdPlanner/Pages/Tasks/Create.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Tasks
{
    public class CreateModel : PageModel
    {
        private readonly PlannerDbContext _db;

        public CreateModel(PlannerDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public PlannerTask TaskItem { get; set; } = new PlannerTask();

        public SelectList UserOptions { get; private set; } = default!;

        public async Task OnGetAsync()
        {
            await LoadUserOptionsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadUserOptionsAsync();
                return Page();
            }

            _db.PlannerTasks.Add(TaskItem);
            await _db.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadUserOptionsAsync()
        {
            var users = await _db.Users
                .OrderBy(u => u.SortOrder ?? int.MaxValue)
                .ThenBy(u => u.Name)
                .ToListAsync();

            UserOptions = new SelectList(users, "Id", "Name");
        }
    }
}