// File: src/HouseholdPlanner/Data/DbSeeder.cs
using HouseholdPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseholdPlanner.Data
{

    public static class DbSeeder
    {
        public static async Task SeedAsync(PlannerDbContext context, CancellationToken cancellationToken = default)
        {
            // Only seed when database is empty for our domain.
            if (await context.Users.AnyAsync(cancellationToken))
            {
                return;
            }

            // --- Users ---
            var users = new[]
            {
            new User { Name = "Cornelis", ColorHex = "#ef4444" },
            new User { Name = "Melissa", ColorHex = "#3b82f6" },
            new User { Name = "Waylon",  ColorHex = "#22c55e" }
        };

            context.Users.AddRange(users);
            await context.SaveChangesAsync(cancellationToken);

            // --- Tasks + Subtasks ---
            var tasks = new[]
            {
            new PlannerTask
            {
                Name = "Vacuum downstairs",
                Description = "Living room, hallway and kitchen."
            },
            new PlannerTask
            {
                Name = "Empty dishwasher",
                Description = "Empty and put everything away."
            },
            new PlannerTask
            {
                Name = "Take out garbage",
                Description = "General waste, plastic and paper as needed."
            }
        };

            context.Tasks.AddRange(tasks);
            await context.SaveChangesAsync(cancellationToken);

            var subtasks = new[]
            {
            new Subtask { PlannerTaskId = tasks[0].Id, Name = "Vacuum living room"},
            new Subtask { PlannerTaskId = tasks[0].Id, Name = "Vacuum hallway"},
            new Subtask { PlannerTaskId = tasks[0].Id, Name = "Vacuum kitchen"},

            new Subtask { PlannerTaskId = tasks[1].Id, Name = "Empty top rack"},
            new Subtask { PlannerTaskId = tasks[1].Id, Name = "Empty bottom rack" },
            new Subtask { PlannerTaskId = tasks[1].Id, Name = "Cutlery basket" },

            new Subtask { PlannerTaskId = tasks[2].Id, Name = "Collect bins" },
            new Subtask { PlannerTaskId = tasks[2].Id, Name = "Sort recycling" },
            new Subtask { PlannerTaskId = tasks[2].Id, Name = "Take bins outside" }
        };

            context.Subtasks.AddRange(subtasks);
            await context.SaveChangesAsync(cancellationToken);

            // --- Availability (simple weekday evening example) ---
            var weekdayEveningSlots = new List<AvailabilitySlot>();
            foreach (var user in users)
            {
                foreach (var day in new[]
                         { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
                {
                    weekdayEveningSlots.Add(new AvailabilitySlot
                    {
                        User = user,
                        DayOfWeek = day,
                        StartLocalTime = new TimeOnly(18, 0),
                        EndLocalTime = new TimeOnly(21, 0)
                    });
                }
            }

            context.AvailabilitySlots.AddRange(weekdayEveningSlots);
            await context.SaveChangesAsync(cancellationToken);

            // --- Meals, Tags, and Plans ---

            var tags = new[]
            {
            new MealTag { Name = "Pasta", TagType = MealTagType.Content },
            new MealTag { Name = "Chicken", TagType = MealTagType.Content },
            new MealTag { Name = "Fish", TagType = MealTagType.Content },
            new MealTag { Name = "Veggie", TagType = MealTagType.Content },
            new MealTag { Name = "Quick", TagType = MealTagType.PrepTime },
            new MealTag { Name = "Slow", TagType = MealTagType.PrepTime },
            new MealTag { Name = "Summer", TagType = MealTagType.Season },
            new MealTag { Name = "Winter", TagType = MealTagType.Season },
            new MealTag { Name = "RainyDay", TagType = MealTagType.Weather }
        };

            context.MealTags.AddRange(tags);
            await context.SaveChangesAsync(cancellationToken);

            var meals = new[]
            {
            new Meal
            {
                Name = "Spaghetti Bolognese",
                Description = "Pasta with slow-cooked meat sauce."
            },
            new Meal
            {
                Name = "Chicken stir-fry with rice",
                Description = "Quick wok dish with vegetables."
            },
            new Meal
            {
                Name = "Veggie curry with naan",
                Description = "Comforting vegetarian curry."
            },
            new Meal
            {
                Name = "Oven-baked salmon with potatoes",
                Description = "Simple tray bake."
            },
            new Meal
            {
                Name = "Homemade pizza night",
                Description = "Make-your-own toppings."
            }
        };

            context.Meals.AddRange(meals);
            await context.SaveChangesAsync(cancellationToken);

            // Map meals to tags
            var tagLookup = tags.ToDictionary(t => t.Name);

            var mappings = new List<MealTagMap>
        {
            // Spaghetti Bolognese
            new() { MealId = meals[0].Id, MealTagId = tagLookup["Pasta"].Id },
            new() { MealId = meals[0].Id, MealTagId = tagLookup["Slow"].Id },
            new() { MealId = meals[0].Id, MealTagId = tagLookup["Winter"].Id },
            new() { MealId = meals[0].Id, MealTagId = tagLookup["RainyDay"].Id },

            // Chicken stir-fry
            new() { MealId = meals[1].Id, MealTagId = tagLookup["Chicken"].Id },
            new() { MealId = meals[1].Id, MealTagId = tagLookup["Quick"].Id },
            new() { MealId = meals[1].Id, MealTagId = tagLookup["Summer"].Id },

            // Veggie curry
            new() { MealId = meals[2].Id, MealTagId = tagLookup["Veggie"].Id },
            new() { MealId = meals[2].Id, MealTagId = tagLookup["Slow"].Id },
            new() { MealId = meals[2].Id, MealTagId = tagLookup["Winter"].Id },

            // Salmon tray bake
            new() { MealId = meals[3].Id, MealTagId = tagLookup["Fish"].Id },
            new() { MealId = meals[3].Id, MealTagId = tagLookup["Quick"].Id },

            // Pizza night
            new() { MealId = meals[4].Id, MealTagId = tagLookup["Pasta"].Id },
            new() { MealId = meals[4].Id, MealTagId = tagLookup["Quick"].Id },
            new() { MealId = meals[4].Id, MealTagId = tagLookup["Summer"].Id }
        };

            context.MealTagMaps.AddRange(mappings);
            await context.SaveChangesAsync(cancellationToken);

            // --- Meal plan for the coming week (dinners only) ---

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            var monday = today.AddDays(daysUntilMonday);

            var mealPlans = new[]
            {
            new MealPlan { Date = monday,            MealId = meals[0].Id },
            new MealPlan { Date = monday.AddDays(1), MealId = meals[1].Id },
            new MealPlan { Date = monday.AddDays(2), MealId = meals[2].Id },
            new MealPlan { Date = monday.AddDays(3), MealId = meals[3].Id },
            new MealPlan { Date = monday.AddDays(4), MealId = meals[4].Id }
        };

            context.MealPlans.AddRange(mealPlans);
            await context.SaveChangesAsync(cancellationToken);

            // --- Sample task schedules for the same week ---

            var schedules = new List<TaskSchedule>
        {
            new() {
                Id = tasks[0].Id,
                UserId = users[0].Id,
                Date = monday,
                StartLocalTime = new TimeOnly(18, 0),
                AmountOfTime = TimeSpan.FromMinutes(45),
                IsCompleted = false
            },
            new() {
                Id = tasks[1].Id,
                UserId = users[1].Id,
                Date = monday.AddDays(1),
                StartLocalTime = new TimeOnly(19, 0),
                AmountOfTime = TimeSpan.FromMinutes(20),
                IsCompleted = false
            },
            new() {
                Id = tasks[2].Id,
                UserId = users[2].Id,
                Date = monday.AddDays(2),
                StartLocalTime = new TimeOnly(20, 0),
                AmountOfTime = TimeSpan.FromMinutes(30),
                IsCompleted = false
            }
        };

            context.TaskSchedules.AddRange(schedules);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}