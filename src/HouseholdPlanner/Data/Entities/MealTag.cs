// File: src/HouseholdPlanner/Data/Entities/MealTag.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HouseholdPlanner.Data.Entities
{
    public class MealTag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public MealTagType TagType { get; set; }

        public ICollection<MealTagMap> MealTagMappings { get; set; } = new List<MealTagMap>();
    }

}
