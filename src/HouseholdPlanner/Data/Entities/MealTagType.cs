namespace HouseholdPlanner.Data.Entities
{
    /// <summary>
    /// High-level type/category for a meal tag.
    /// Focused on dinner characteristics: contents, prep-time, season, weather.
    /// </summary>
    public enum MealTagType
    {
        Content = 0,       // e.g. "Pasta", "Chicken", "Veggie"
        PrepTime = 1,      // e.g. "Quick", "Slow"
        Season = 2,        // e.g. "Summer", "Winter"
        Weather = 3        // e.g. "RainyDay", "HotWeather"
    }

}
