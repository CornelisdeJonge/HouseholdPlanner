// File: src/HouseholdPlanner/Pages/Users/Index.cshtml.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly PlannerDbContext _context;

        public IndexModel(PlannerDbContext context)
        {
            _context = context;
        }

        public IList<User> Users { get; private set; } = new List<User>();

        public async Task OnGetAsync()
        {
            Users = await _context.Users
                .OrderBy(u => !u.SortOrder.HasValue)      // users with SortOrder first
                .ThenBy(u => u.SortOrder)
                .ThenBy(u => u.Name)
                .AsNoTracking()
                .ToListAsync();
        }
    }

}
