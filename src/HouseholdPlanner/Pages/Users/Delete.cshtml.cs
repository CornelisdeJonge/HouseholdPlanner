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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return RedirectToPage("Index");
            }

            PlannerUser = user;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }

}
