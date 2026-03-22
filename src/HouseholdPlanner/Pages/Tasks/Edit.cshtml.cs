// File: src/HouseholdPlanner/Pages/Tasks/Edit.cshtml.cs
using System.Linq;
using System.Threading.Tasks;
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Tasks
{
    public class EditModel : PageModel
    {
        private readonly PlannerDbContext _db;

        public EditModel(PlannerDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public PlannerTask TaskItem { get; set; } = default!;

        public SelectList UserOptions { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var task = await _db.Tasks
                .Include(t => t.Subtasks)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            TaskItem = task;
            await LoadUserOptionsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadUserOptionsAsync();
                TaskItem.Subtasks = await _db.Subtasks
                    .Where(s => s.PlannerTaskId == TaskItem.Id)
                    .ToListAsync();
                return Page();
            }

            var existing = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == TaskItem.Id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = TaskItem.Name;
            existing.Description = TaskItem.Description;
            existing.Priority = TaskItem.Priority;
            existing.Deadline = TaskItem.Deadline;
            existing.AssignedUserId = TaskItem.AssignedUserId;

            await _db.SaveChangesAsync();

            // ✅ Use explicit absolute page path to ensure redirect back to overview
            return RedirectToPage("/Tasks/Index");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostChangeAssigneeAsync(int id)
        {
            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            if (!await TryUpdateModelAsync(task, "TaskItem", t => t.AssignedUserId))
            {
                return BadRequest();
            }

            await _db.SaveChangesAsync();
            return new EmptyResult();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddSubtaskAsync(int id, string newSubtaskTitle)
        {
            // For this handler, we only care about the new subtask title.
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(newSubtaskTitle))
            {
                ModelState.AddModelError("NewSubtaskTitle", "Subtask title is required.");
            }

            var task = await _db.Tasks
                .Include(t => t.Subtasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TaskItem = task;
                await LoadUserOptionsAsync();
                return Page();
            }

            _db.Subtasks.Add(new Subtask
            {
                PlannerTaskId = task.Id,
                Name = newSubtaskTitle.Trim(),
                IsDone = false
            });

            await _db.SaveChangesAsync();

            return RedirectToPage(new { id });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostToggleSubtaskAsync(int subtaskId)
        {
            var subtask = await _db.Subtasks.FirstOrDefaultAsync(s => s.Id == subtaskId);
            if (subtask == null)
            {
                return NotFound();
            }

            subtask.IsDone = !subtask.IsDone;
            await _db.SaveChangesAsync();

            return new EmptyResult();
        }

        private async Task LoadUserOptionsAsync()
        {
            var users = await _db.Users
                .OrderBy(u => u.SortOrder ?? int.MaxValue)
                .ThenBy(u => u.Name)
                .ToListAsync();

            UserOptions = new SelectList(users, "Id", "Name");
        }
    }
}