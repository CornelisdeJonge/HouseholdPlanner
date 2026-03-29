// File: src/HouseholdPlanner/Pages/Users/Create.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HouseholdPlanner.Pages.Users
{
    public class CreateModel(PlannerDbContext context) : PageModel
    {
        [BindProperty]
        public PlannerUser PlannerUser { get; set; } = new PlannerUser();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            context.Users.Add(PlannerUser);
            await context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}