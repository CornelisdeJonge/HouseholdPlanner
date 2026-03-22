// File: src/HouseholdPlanner/Data/Entities/Meal.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Data.Entities
{
    public class Meal
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public ICollection<MealTagMap> MealTagMappings { get; set; } = new List<MealTagMap>();
        public ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();
    }

}
