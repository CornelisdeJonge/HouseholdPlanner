// File: src/HouseholdPlanner/Pages/Users/Delete.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Users
{
    public class DeleteModel(PlannerDbContext context) : PageModel
    {
        [BindProperty]
        public PlannerUser PlannerUser { get; set; } = null!;

        public IList<PlannerTask> AssignedTasks { get; set; } = new List<PlannerTask>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return RedirectToPage("Index");
            }

            PlannerUser = user;

            AssignedTasks = await context.PlannerTasks
                .AsNoTracking()
                .Where(t => t.AssigneeId == id)
                .OrderBy(t => t.Deadline)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return RedirectToPage("Index");
            }

            // Unassign all tasks that are currently assigned to this user
            var tasks = await context.PlannerTasks
                .Where(t => t.AssigneeId == id)
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.AssigneeId = null;
            }

            context.Users.Remove(user);

            await context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}