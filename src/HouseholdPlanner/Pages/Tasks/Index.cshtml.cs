// File: src/HouseholdPlanner/Pages/Tasks/Index.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Tasks
{
    public class IndexModel : PageModel
    {
        private readonly PlannerDbContext _db;

        public IndexModel(PlannerDbContext db)
        {
            _db = db;
        }

        public IList<PlannerTask> Tasks { get; private set; } = new List<PlannerTask>();

        public async Task OnGetAsync()
        {
            Tasks = await _db.PlannerTasks
                .Include(t => t.Assignee)
                .Include(t => t.Subtasks)
                .OrderBy(t => t.Deadline ?? DateOnly.MaxValue)
                .ThenByDescending(t => t.Priority)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }
    }
}