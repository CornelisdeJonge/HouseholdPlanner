// File: src/HouseholdPlanner/Models/InputModel.cs
using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Models
{
    // Shared input model for scheduling task time blocks
    public class InputModel
    {
        // For Edit/Delete scenarios; Create pages can ignore this
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeOnly StartLocalTime { get; set; }

        /// <summary>
        /// Duration in minutes; rounded to 30‑minute increments (30–480).
        /// </summary>
        [Range(30, 480)]
        public int DurationMinutes { get; set; } = 60;

        public string? ReturnUrl { get; set; }
    }
}