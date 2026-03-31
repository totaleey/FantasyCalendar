using FantasyCalendar.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyCalendar.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<Month> Months { get; set; }
    public DbSet<Weekday> Weekdays { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<RecurrencePattern> RecurrencePatterns { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Unavailability> Unavailabilities { get; set; }
    public DbSet<EventCharacter> EventCharacters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure many-to-many
        modelBuilder.Entity<EventCharacter>()
            .HasKey(ec => new { ec.EventId, ec.CharacterId });

        // Configure recurring pattern as owned entity
        modelBuilder.Entity<RecurrencePattern>()
            .HasOne(r => r.Event)
            .WithOne(e => e.Recurrence)
            .HasForeignKey<RecurrencePattern>(r => r.EventId);

        // Seed the Eorzean calendar
        modelBuilder.Entity<Calendar>().HasData(new Calendar
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Eorzean Calendar",
            Description = "The six-astral and six-umbral era calendar of Hydaelyn. Each year consists of 12 months of 32 days, totaling 384 days per year.",
            DaysPerYear = 384,
            MonthsPerYear = 12,
            DaysPerWeek = 8
        });
        var calendarId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var months = new[]
        {
            new Month { Id = Guid.NewGuid(), Name = "First Umbral Moon", Order = 1, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "First Astral Moon", Order = 2, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Second Umbral Moon", Order = 3, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Second Astral Moon", Order = 4, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Third Umbral Moon", Order = 5, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Third Astral Moon", Order = 6, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Fourth Umbral Moon", Order = 7, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Fourth Astral Moon", Order = 8, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Fifth Umbral Moon", Order = 9, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Fifth Astral Moon", Order = 10, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Sixth Umbral Moon", Order = 11, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.NewGuid(), Name = "Sixth Astral Moon", Order = 12, DaysInMonth = 32, CalendarId = calendarId }
        };
        modelBuilder.Entity<Month>().HasData(months);

        modelBuilder.Entity<Weekday>().HasData(new[]
        {
            new Weekday { Id = Guid.NewGuid(), Name = "Sun", Order = 1, CalendarId = calendarId },
            new Weekday { Id = Guid.NewGuid(), Name = "Moon", Order = 2, CalendarId = calendarId },
            new Weekday { Id = Guid.NewGuid(), Name = "Fire", Order = 3, CalendarId = calendarId },
            new Weekday { Id = Guid.NewGuid(), Name = "Water", Order = 4, CalendarId = calendarId },
            new Weekday { Id = Guid.NewGuid(), Name = "Wind", Order = 5, CalendarId = calendarId },
            new Weekday { Id = Guid.NewGuid(), Name = "Lightning", Order = 6, CalendarId = calendarId },
            new Weekday { Id = Guid.NewGuid(), Name = "Ice", Order = 7, CalendarId = calendarId },
            new Weekday { Id = Guid.NewGuid(), Name = "Earth", Order = 8, CalendarId = calendarId }
        });
    }
}