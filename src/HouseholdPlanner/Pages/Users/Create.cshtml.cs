// File: src/HouseholdPlanner/Pages/Users/Create.cshtml.cs
using System.Threading.Tasks;
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HouseholdPlanner.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly PlannerDbContext _context;

        public CreateModel(PlannerDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User User { get; set; } = new User();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}