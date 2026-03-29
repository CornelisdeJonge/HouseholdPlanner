// File: src/HouseholdPlanner/Data/Entities/MealPlan.cs
namespace HouseholdPlanner.Data.Entities
{

    /// <summary>
    /// Planned dinner for a specific date. Only dinners are planned,
    /// so there is no separate MealType for breakfast/lunch.
    /// </summary>
    public class MealPlan
    {
        public int Id { get; set; }

        /// <summary>
        /// Local date for the planned dinner.
        /// </summary>
        public DateOnly Date { get; set; }

        public int MealId { get; set; }

        public Meal Meal { get; set; } = null!;
    }

}
