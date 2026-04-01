using FantasyCalendar.API.Services;
using FantasyCalendar.Core.Models;
using FantasyCalendar.Infrastructure.Data;
using FantasyCalendar.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FantasyCalendar.Tests.Services;

public class CalendarServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CalendarService _service;

    public CalendarServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _service = new CalendarService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllCalendarsAsync_ShouldReturnAllCalendars()
    {
        // Act
        var result = await _service.GetAllCalendarsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Eorzean Calendar");
    }

    [Fact]
    public async Task GetCalendarByIdAsync_WithValidId_ShouldReturnCalendar()
    {
        // Act
        var result = await _service.GetCalendarByIdAsync(TestData.TestCalendarId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(TestData.TestCalendarId);
        result.Name.Should().Be("Eorzean Calendar");
    }

    [Fact]
    public async Task GetCalendarByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetCalendarByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCalendarAsync_ShouldAddNewCalendar()
    {
        // Arrange
        var newCalendar = new Calendar
        {
            Name = "Test Calendar",
            Description = "Test Description",
            DaysPerYear = 365,
            MonthsPerYear = 12,
            DaysPerWeek = 7
        };

        // Act
        var result = await _service.CreateCalendarAsync(newCalendar);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Calendar");

        var saved = await _context.Calendars.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateCalendarAsync_WithValidId_ShouldUpdateCalendar()
    {
        // Arrange
        var updatedCalendar = new Calendar
        {
            Name = "Updated Calendar",
            Description = "Updated Description",
            DaysPerYear = 360,
            MonthsPerYear = 12,
            DaysPerWeek = 7
        };

        // Act
        var result = await _service.UpdateCalendarAsync(TestData.TestCalendarId, updatedCalendar);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Calendar");
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateCalendarAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var updatedCalendar = new Calendar
        {
            Name = "Updated Calendar",
            Description = "Updated Description",
            DaysPerYear = 360,
            MonthsPerYear = 12,
            DaysPerWeek = 7
        };

        // Act
        var result = await _service.UpdateCalendarAsync(Guid.NewGuid(), updatedCalendar);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCalendarAsync_WithValidId_ShouldRemoveCalendar()
    {
        // Act
        var result = await _service.DeleteCalendarAsync(TestData.TestCalendarId);

        // Assert
        result.Should().BeTrue();

        var deleted = await _context.Calendars.FindAsync(TestData.TestCalendarId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCalendarAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.DeleteCalendarAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CalendarExistsAsync_WithValidId_ShouldReturnTrue()
    {
        // Act
        var result = await _service.CalendarExistsAsync(TestData.TestCalendarId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CalendarExistsAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.CalendarExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}