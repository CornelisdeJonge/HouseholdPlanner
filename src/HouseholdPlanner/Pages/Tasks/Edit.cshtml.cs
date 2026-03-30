// File: src/HouseholdPlanner/Pages/Tasks/Edit.cshtml.cs
using HouseholdPlanner.Data;
using HouseholdPlanner.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Pages.Tasks
{
    public class EditModel(PlannerDbContext db) : PageModel
    {
        [BindProperty]
        public PlannerTask TaskItem { get; set; } = default!;

        public SelectList UserOptions { get; private set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var task = await db.PlannerTasks
                .Include(t => t.Subtasks)
                .Include(t => t.Assignee)
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
                TaskItem.Subtasks = await db.Subtasks
                    .Where(s => s.PlannerTaskId == TaskItem.Id)
                    .ToListAsync();
                return Page();
            }

            var existing = await db.PlannerTasks.FirstOrDefaultAsync(t => t.Id == TaskItem.Id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = TaskItem.Name;
            existing.Description = TaskItem.Description;
            existing.Priority = TaskItem.Priority;
            existing.Deadline = TaskItem.Deadline;
            existing.AssigneeId = TaskItem.AssigneeId;

            await db.SaveChangesAsync();

            // Redirect back to tasks overview
            return RedirectToPage("/Tasks/Index");
        }

        public async Task<IActionResult> OnPostChangeAssigneeAsync(int id)
        {
            var task = await db.PlannerTasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            if (!await TryUpdateModelAsync(task, "TaskItem", t => t.AssigneeId))
            {
                return BadRequest();
            }

            await db.SaveChangesAsync();
            return new EmptyResult();
        }

        public async Task<IActionResult> OnPostAddSubtaskAsync(int id, string newSubtaskTitle)
        {
            // For this handler, we only care about the new subtask title.
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(newSubtaskTitle))
            {
                ModelState.AddModelError("NewSubtaskTitle", "Subtask title is required.");
            }

            var task = await db.PlannerTasks
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

            db.Subtasks.Add(new Subtask
            {
                PlannerTaskId = task.Id,
                Name = newSubtaskTitle.Trim(),
                IsDone = false
            });

            await db.SaveChangesAsync();

            return RedirectToPage(new { id });
        }
        public async Task<IActionResult> OnPostToggleSubtaskAsync(int subtaskId)
        {
            var subtask = await db.Subtasks.FirstOrDefaultAsync(s => s.Id == subtaskId);
            if (subtask == null)
            {
                return NotFound();
            }

            subtask.IsDone = !subtask.IsDone;
            await db.SaveChangesAsync();

            var taskId = subtask.PlannerTaskId;

            var task = await db.PlannerTasks
                .Include(t => t.Subtasks)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            return Partial("_SubtasksList", task!);
        }
        public async Task<IActionResult> OnPostDeleteSubtaskAsync(int subtaskId)
        {
            var subtask = await db.Subtasks.FirstOrDefaultAsync(s => s.Id == subtaskId);
            if (subtask == null)
            {
                return NotFound();
            }

            var taskId = subtask.PlannerTaskId;

            db.Subtasks.Remove(subtask);
            await db.SaveChangesAsync();

            // Reload the task with updated subtasks
            var task = await db.PlannerTasks
                .Include(t => t.Subtasks)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            // Return only the subtasks section for HTMX to swap in
            return Partial("_SubtasksList", task);
        }

        private async Task LoadUserOptionsAsync()
        {
            var users = await db.Users
                .OrderBy(u => u.SortOrder ?? int.MaxValue)
                .ThenBy(u => u.Name)
                .ToListAsync();

            // Simple SelectList: value = Id, text = Name, selected = TaskItem.AssigneeId
            UserOptions = new SelectList(
                users,
                nameof(PlannerUser.Id),
                nameof(PlannerUser.Name),
                TaskItem?.AssigneeId
            );
        }
    }
}