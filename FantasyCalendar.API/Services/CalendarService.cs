// CalendarService.cs
using FantasyCalendar.Core.Models;
using FantasyCalendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FantasyCalendar.API.Services;

public class CalendarService : ICalendarService
{
    private readonly AppDbContext _context;

    public CalendarService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Calendar>> GetAllCalendarsAsync(bool includeDetails = false)
    {
        var query = _context.Calendars.AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(c => c.Months)
                .Include(c => c.Weekdays);
        }

        return await query.ToListAsync();
    }

    public async Task<Calendar?> GetCalendarByIdAsync(Guid id, bool includeDetails = true)
    {
        var query = _context.Calendars.AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(c => c.Months)
                .Include(c => c.Weekdays)
                .Include(c => c.Events)
                .Include(c => c.Characters);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Calendar> CreateCalendarAsync(Calendar calendar)
    {
        calendar.Id = Guid.NewGuid();

        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync();

        return calendar;
    }

    public async Task<Calendar?> UpdateCalendarAsync(Guid id, Calendar updatedCalendar)
    {
        var existingCalendar = await _context.Calendars.FindAsync(id);

        if (existingCalendar == null)
        {
            return null;
        }

        existingCalendar.Name = updatedCalendar.Name;
        existingCalendar.Description = updatedCalendar.Description;
        existingCalendar.DaysPerYear = updatedCalendar.DaysPerYear;
        existingCalendar.MonthsPerYear = updatedCalendar.MonthsPerYear;
        existingCalendar.DaysPerWeek = updatedCalendar.DaysPerWeek;

        await _context.SaveChangesAsync();

        return existingCalendar;
    }

    public async Task<bool> DeleteCalendarAsync(Guid id)
    {
        var calendar = await _context.Calendars.FindAsync(id);

        if (calendar == null)
        {
            return false;
        }

        _context.Calendars.Remove(calendar);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CalendarExistsAsync(Guid id)
    {
        return await _context.Calendars.AnyAsync(c => c.Id == id);
    }
}