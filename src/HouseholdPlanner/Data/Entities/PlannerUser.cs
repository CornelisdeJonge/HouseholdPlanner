// File: src/HouseholdPlanner/Data/Entities/User.cs
using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Data.Entities
{
    public class PlannerUser
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Hex color (e.g. #ef4444) used for calendar highlighting.
        /// </summary>

        [RegularExpression("^#([0-9A-Fa-f]{6})$",
            ErrorMessage = "Please enter a valid color like #A1B2C3.")]
        [MaxLength(9)]
        public string? Color { get; set; }

        public bool IsActive { get; set; } = true;

        public int? SortOrder { get; set; }

        public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = [];
        public ICollection<TaskSchedule> TaskSchedules { get; set; } = [];
    }

}
