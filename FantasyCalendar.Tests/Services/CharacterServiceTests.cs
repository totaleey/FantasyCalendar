using FantasyCalendar.API.Services;
using FantasyCalendar.Core.Models;
using FantasyCalendar.Infrastructure.Data;
using FantasyCalendar.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FantasyCalendar.Tests.Services;

public class CharacterServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CharacterService _service;
    private readonly CalendarService _calendarService;

    public CharacterServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _service = new CharacterService(_context);
        _calendarService = new CalendarService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateCharacterAsync_ShouldAddNewCharacter()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Character",
            Description = "Test Description"
        };

        // Act
        var result = await _service.CreateCharacterAsync(TestData.TestCalendarId, character);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Character");

        var saved = await _context.Characters.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCharacterAsync_WithInvalidCalendar_ShouldThrowException()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Character",
            Description = "Test Description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateCharacterAsync(Guid.NewGuid(), character));
    }

    [Fact]
    public async Task GetCharacterByIdAsync_WithValidId_ShouldReturnCharacter()
    {
        // Arrange
        var character = new Character
        {
            Name = "Test Character",
            Description = "Test Description"
        };
        var created = await _service.CreateCharacterAsync(TestData.TestCalendarId, character);

        // Act
        var result = await _service.GetCharacterByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Character");
    }

    [Fact]
    public async Task AddUnavailabilityAsync_ShouldAddUnavailability()
    {
        // Arrange
        var character = new Character { Name = "Test Character" };
        var created = await _service.CreateCharacterAsync(TestData.TestCalendarId, character);

        var unavailability = new Unavailability
        {
            StartDay = 10,
            EndDay = 20,
            Reason = "Testing"
        };

        // Act
        var result = await _service.AddUnavailabilityAsync(created.Id, unavailability);

        // Assert
        result.Should().NotBeNull();
        result.StartDay.Should().Be(10);
        result.EndDay.Should().Be(20);

        var updated = await _service.GetCharacterByIdAsync(created.Id);
        updated!.Unavailabilities.Should().HaveCount(1);
    }

    [Fact]
    public async Task IsCharacterAvailableAsync_WithNoConflicts_ShouldReturnTrue()
    {
        // Arrange
        var character = new Character { Name = "Test Character" };
        var created = await _service.CreateCharacterAsync(TestData.TestCalendarId, character);

        // Act
        var result = await _service.IsCharacterAvailableAsync(created.Id, 1, 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCharacterAvailableAsync_WithConflict_ShouldReturnFalse()
    {
        // Arrange
        var character = new Character { Name = "Test Character" };
        var created = await _service.CreateCharacterAsync(TestData.TestCalendarId, character);

        var unavailability = new Unavailability
        {
            StartDay = 10,
            EndDay = 20,
            Reason = "Busy"
        };
        await _service.AddUnavailabilityAsync(created.Id, unavailability);

        // Act
        var result = await _service.IsCharacterAvailableAsync(created.Id, 15, 25);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCharacterAsync_ShouldRemoveCharacterAndAssociations()
    {
        // Arrange
        var character = new Character { Name = "Test Character" };
        var created = await _service.CreateCharacterAsync(TestData.TestCalendarId, character);

        var unavailability = new Unavailability
        {
            StartDay = 10,
            EndDay = 20,
            Reason = "Busy"
        };
        await _service.AddUnavailabilityAsync(created.Id, unavailability);

        // Act
        var result = await _service.DeleteCharacterAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        var deleted = await _service.GetCharacterByIdAsync(created.Id);
        deleted.Should().BeNull();

        var unavailabilities = await _context.Unavailabilities.ToListAsync();
        unavailabilities.Should().BeEmpty();
    }
}