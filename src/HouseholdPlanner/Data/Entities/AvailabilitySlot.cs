// File: src/HouseholdPlanner/Data/Entities/AvailabilitySlot.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Data.Entities
{

    /// <summary>
    /// Recurring weekly availability for a user (local time, Monday-start week logic lives in queries).
    /// </summary>
    public class AvailabilitySlot
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeOnly StartLocalTime { get; set; }

        [Required]
        public TimeOnly EndLocalTime { get; set; }
    }

}
