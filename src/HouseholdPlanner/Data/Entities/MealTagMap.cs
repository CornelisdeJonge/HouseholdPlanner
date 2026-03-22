namespace HouseholdPlanner.Data.Entities
{

    /// <summary>
    /// Many-to-many join entity between Meal and MealTag.
    /// </summary>
    public class MealTagMap
    {
        public int MealId { get; set; }

        public Meal Meal { get; set; } = null!;

        public int MealTagId { get; set; }

        public MealTag MealTag { get; set; } = null!;
    }

}
