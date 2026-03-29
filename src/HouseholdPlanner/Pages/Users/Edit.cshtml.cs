// File: src/HouseholdPlanner/Pages/Users/Edit.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Users
{

    public class EditModel(PlannerDbContext context) : PageModel
    {
        [BindProperty]
        public PlannerUser PlannerUser { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return RedirectToPage("Index");
            }

            PlannerUser = user;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userToUpdate = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userToUpdate == null)
            {
                return RedirectToPage("Index");
            }

            if (!await TryUpdateModelAsync(
                    userToUpdate,
                    "User",
                    u => u.Name,
                    u => u.Color,
                    u => u.SortOrder,
                    u => u.IsActive))
            {
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}