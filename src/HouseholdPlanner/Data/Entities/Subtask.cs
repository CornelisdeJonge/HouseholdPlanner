// File: src/HouseholdPlanner/Data/Entities/Subtask.cs
using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Data.Entities
{
    public class Subtask
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public bool IsDone { get; set; }

        public int PlannerTaskId { get; set; }
        public PlannerTask PlannerTask { get; set; } = default!;
    }
}