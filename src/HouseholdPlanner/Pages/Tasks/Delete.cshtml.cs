// File: src/HouseholdPlanner/Pages/Tasks/Delete.cshtml.cs
using System.Threading.Tasks;
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Tasks
{
    public class DeleteModel : PageModel
    {
        private readonly PlannerDbContext _db;

        public DeleteModel(PlannerDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public PlannerTask TaskItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var task = await _db.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            TaskItem = task;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
            {
                return RedirectToPage("./Index");
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}