// File: src/HouseholdPlanner/Pages/Users/Index.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Users
{
    public class IndexModel(PlannerDbContext context) : PageModel
    {
        public IList<PlannerUser> Users { get; private set; } = new List<PlannerUser>();

        public async Task OnGetAsync()
        {
            Users = await context.Users
                .OrderBy(u => !u.SortOrder.HasValue)      // users with SortOrder first
                .ThenBy(u => u.SortOrder)
                .ThenBy(u => u.Name)
                .AsNoTracking()
                .ToListAsync();
        }
    }

}
