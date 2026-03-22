// File: src/HouseholdPlanner/Pages/Users/Edit.cshtml.cs
using System.Linq;
using System.Threading.Tasks;
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Users
{

    public class EditModel : PageModel
    {
        private readonly PlannerDbContext _context;

        public EditModel(PlannerDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User User { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return RedirectToPage("Index");
            }

            User = user;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userToUpdate == null)
            {
                return RedirectToPage("Index");
            }

            if (!await TryUpdateModelAsync(
                    userToUpdate,
                    "User",
                    u => u.Name,
                    u => u.ColorHex,
                    u => u.SortOrder,
                    u => u.IsActive))
            {
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}