using HouseholdPlanner.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Tasks
{
    public class EditAssigneeModel(PlannerDbContext db) : PageModel
    {
        [BindProperty]
        public int TaskId { get; set; }

        [BindProperty]
        public int? AssigneeId { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var task = await db.PlannerTasks.FirstOrDefaultAsync(t => t.Id == TaskId);
            if (task == null)
                return NotFound();

            task.AssigneeId = AssigneeId;
            await db.SaveChangesAsync();

            var newLabel = task.AssigneeId == null
                ? "Assigned to nobody"
                : $"Assigned to {db.Users.First(u => u.Id == task.AssigneeId).Name}";

            return Content(newLabel);
        }
    }
}