using FantasyCalendar.API.Services;
using FantasyCalendar.Core.Models;
using FantasyCalendar.Infrastructure.Data;
using FantasyCalendar.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FantasyCalendar.Tests.Services;

public class EventServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly EventService _service;

    public EventServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _service = new EventService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateEventAsync_ShouldAddOneTimeEvent()
    {
        // Arrange
        var newEvent = new Event
        {
            Title = "Test Event",
            Description = "Test Description",
            StartDay = 10,
            EndDay = 12,
            Recurrence = null
        };

        // Act
        var result = await _service.CreateEventAsync(TestData.TestCalendarId, newEvent);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test Event");
        result.Recurrence.Should().BeNull();

        var saved = await _context.Events.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateEventAsync_ShouldAddRecurringEvent()
    {
        // Arrange
        var newEvent = new Event
        {
            Title = "Recurring Event",
            Description = "Test Description",
            StartDay = 7,
            EndDay = 7,
            Recurrence = new RecurrencePattern
            {
                Type = RecurrenceType.Weekly,
                Interval = 1,
                DayOfWeek = 1,
                MaxOccurrences = 10
            }
        };

        // Act
        var result = await _service.CreateEventAsync(TestData.TestCalendarId, newEvent);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Recurrence.Should().NotBeNull();
        result.Recurrence!.Type.Should().Be(RecurrenceType.Weekly);

        var saved = await _context.Events
            .Include(e => e.Recurrence)
            .FirstOrDefaultAsync(e => e.Id == result.Id);
        saved!.Recurrence.Should().NotBeNull();
    }

    [Fact]
    public async Task ExpandRecurrenceAsync_ForWeeklyEvent_ShouldReturnCorrectDays()
    {
        // Arrange
        var newEvent = new Event
        {
            Title = "Weekly Meeting",
            Description = "Test",
            StartDay = 7,
            EndDay = 7,
            Recurrence = new RecurrencePattern
            {
                Type = RecurrenceType.Weekly,
                Interval = 1,
                MaxOccurrences = 5
            }
        };

        var created = await _service.CreateEventAsync(TestData.TestCalendarId, newEvent);

        // Act
        var occurrences = await _service.ExpandRecurrenceAsync(created);

        // Assert
        occurrences.Should().HaveCount(5);
        occurrences.Should().BeEquivalentTo(new[] { 7, 14, 21, 28, 35 });
    }

    [Fact]
    public async Task ExpandRecurrenceAsync_ForDailyEvent_ShouldReturnCorrectDays()
    {
        // Arrange
        var newEvent = new Event
        {
            Title = "Daily Event",
            Description = "Test",
            StartDay = 10,
            EndDay = 10,
            Recurrence = new RecurrencePattern
            {
                Type = RecurrenceType.Daily,
                Interval = 3,
                MaxOccurrences = 4
            }
        };

        var created = await _service.CreateEventAsync(TestData.TestCalendarId, newEvent);

        // Act
        var occurrences = await _service.ExpandRecurrenceAsync(created);

        // Assert
        occurrences.Should().HaveCount(4);
        occurrences.Should().BeEquivalentTo(new[] { 10, 13, 16, 19 });
    }

    [Fact]
    public async Task UpdateEventAsync_ShouldChangeFromRecurringToOneTime()
    {
        // Arrange
        var newEvent = new Event
        {
            Title = "Original",
            Description = "Test",
            StartDay = 7,
            EndDay = 7,
            Recurrence = new RecurrencePattern
            {
                Type = RecurrenceType.Weekly,
                Interval = 1,
                MaxOccurrences = 5
            }
        };

        var created = await _service.CreateEventAsync(TestData.TestCalendarId, newEvent);

        var updatedEvent = new Event
        {
            Title = "Updated",
            Description = "Test",
            StartDay = 7,
            EndDay = 7,
            Recurrence = null
        };

        // Act
        var result = await _service.UpdateEventAsync(created.Id, updatedEvent);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated");
        result.Recurrence.Should().BeNull();
    }

    [Fact]
    public async Task DeleteEventAsync_ShouldRemoveEvent()
    {
        // Arrange
        var newEvent = new Event
        {
            Title = "To Delete",
            Description = "Test",
            StartDay = 10,
            EndDay = 12
        };

        var created = await _service.CreateEventAsync(TestData.TestCalendarId, newEvent);

        // Act
        var result = await _service.DeleteEventAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        var deleted = await _service.GetEventByIdAsync(created.Id);
        deleted.Should().BeNull();
    }
}