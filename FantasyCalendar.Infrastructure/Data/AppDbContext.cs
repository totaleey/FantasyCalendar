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

        // Define static GUIDs for seed data
        var calendarId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Seed the Eorzean calendar
        modelBuilder.Entity<Calendar>().HasData(new Calendar
        {
            Id = calendarId,
            Name = "Eorzean Calendar",
            Description = "The six-astral and six-umbral era calendar of Hydaelyn. Each year consists of 12 months of 32 days, totaling 384 days per year.",
            DaysPerYear = 384,
            MonthsPerYear = 12,
            DaysPerWeek = 8
        });

        // Seed months with STATIC GUIDs
        var months = new[]
        {
            new Month { Id = Guid.Parse("22222222-1111-1111-1111-111111111111"), Name = "First Umbral Moon", Order = 1, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-2222-1111-1111-111111111111"), Name = "First Astral Moon", Order = 2, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-3333-1111-1111-111111111111"), Name = "Second Umbral Moon", Order = 3, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-4444-1111-1111-111111111111"), Name = "Second Astral Moon", Order = 4, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-5555-1111-1111-111111111111"), Name = "Third Umbral Moon", Order = 5, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-6666-1111-1111-111111111111"), Name = "Third Astral Moon", Order = 6, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-7777-1111-1111-111111111111"), Name = "Fourth Umbral Moon", Order = 7, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-8888-1111-1111-111111111111"), Name = "Fourth Astral Moon", Order = 8, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-9999-1111-1111-111111111111"), Name = "Fifth Umbral Moon", Order = 9, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-aaaa-1111-1111-111111111111"), Name = "Fifth Astral Moon", Order = 10, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-bbbb-1111-1111-111111111111"), Name = "Sixth Umbral Moon", Order = 11, DaysInMonth = 32, CalendarId = calendarId },
            new Month { Id = Guid.Parse("22222222-cccc-1111-1111-111111111111"), Name = "Sixth Astral Moon", Order = 12, DaysInMonth = 32, CalendarId = calendarId }
        };
        modelBuilder.Entity<Month>().HasData(months);

        // Seed weekdays with STATIC GUIDs
        var weekdays = new[]
        {
            new Weekday { Id = Guid.Parse("33333333-1111-1111-1111-111111111111"), Name = "Sun", Order = 1, CalendarId = calendarId },
            new Weekday { Id = Guid.Parse("33333333-2222-1111-1111-111111111111"), Name = "Moon", Order = 2, CalendarId = calendarId },
            new Weekday { Id = Guid.Parse("33333333-3333-1111-1111-111111111111"), Name = "Fire", Order = 3, CalendarId = calendarId },
            new Weekday { Id = Guid.Parse("33333333-4444-1111-1111-111111111111"), Name = "Water", Order = 4, CalendarId = calendarId },
            new Weekday { Id = Guid.Parse("33333333-5555-1111-1111-111111111111"), Name = "Wind", Order = 5, CalendarId = calendarId },
            new Weekday { Id = Guid.Parse("33333333-6666-1111-1111-111111111111"), Name = "Lightning", Order = 6, CalendarId = calendarId },
            new Weekday { Id = Guid.Parse("33333333-7777-1111-1111-111111111111"), Name = "Ice", Order = 7, CalendarId = calendarId },
            new Weekday { Id = Guid.Parse("33333333-8888-1111-1111-111111111111"), Name = "Earth", Order = 8, CalendarId = calendarId }
        };
        modelBuilder.Entity<Weekday>().HasData(weekdays);
    }
}