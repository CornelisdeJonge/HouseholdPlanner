// File: src/HouseholdPlanner/Pages/Users/Delete.cshtml.cs
using System.Threading.Tasks;
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Users
{

    public class DeleteModel : PageModel
    {
        private readonly PlannerDbContext _context;

        public DeleteModel(PlannerDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User User { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return RedirectToPage("Index");
            }

            User = user;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }

}
