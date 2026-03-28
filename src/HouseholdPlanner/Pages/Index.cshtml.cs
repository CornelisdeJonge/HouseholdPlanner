// File: src/HouseholdPlanner/Pages/Index.cshtml.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HouseholdPlanner.Pages
{
    public class IndexModel : PageModel
    {
        private const int DefaultStartHour = 6;
        private const int DefaultEndHour = 22;

        private readonly List<DateOnly> _days = new();
        private readonly List<TimeOnly> _hourSlots = new();

        public DateOnly WeekStart { get; private set; }
        public DateOnly WeekEnd => WeekStart.AddDays(6);

        public IReadOnlyList<DateOnly> Days => _days;
        public IReadOnlyList<TimeOnly> HourSlots => _hourSlots;

        public int StartHour { get; private set; }
        public int EndHour { get; private set; }

        public void OnGet([FromQuery] DateOnly? weekStart)
        {
            StartHour = DefaultStartHour;
            EndHour = DefaultEndHour;

            var todayLocal = DateOnly.FromDateTime(DateTime.Today);
            var baseDate = weekStart ?? todayLocal;

            WeekStart = GetMonday(baseDate);

            _days.Clear();
            for (var i = 0; i < 7; i++)
            {
                _days.Add(WeekStart.AddDays(i));
            }

            _hourSlots.Clear();
            for (var hour = StartHour; hour < EndHour; hour++)
            {
                _hourSlots.Add(new TimeOnly(hour, 0));
            }
        }

        private static DateOnly GetMonday(DateOnly date)
        {
            // Ensure Monday-based week: Monday = 1, Sunday = 0
            var diff = ((int)date.DayOfWeek + 6) % 7;
            return date.AddDays(-diff);
        }
    }
}