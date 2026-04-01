using FantasyCalendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FantasyCalendar.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        // Seed Eorzean calendar data for tests
        SeedTestData(context);

        return context;
    }

    private static void SeedTestData(AppDbContext context)
    {
        var calendarId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        context.Calendars.Add(new Core.Models.Calendar
        {
            Id = calendarId,
            Name = "Eorzean Calendar",
            Description = "Test calendar",
            DaysPerYear = 384,
            MonthsPerYear = 12,
            DaysPerWeek = 8
        });

        context.SaveChanges();
    }
}