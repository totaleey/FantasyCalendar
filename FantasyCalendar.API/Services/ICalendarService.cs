using FantasyCalendar.Core.Models;

namespace FantasyCalendar.API.Services
{
    public interface ICalendarService
    {
        Task<IEnumerable<Calendar>> GetAllCalendarsAsync(bool includeDetails = false);
        Task<Calendar?> GetCalendarByIdAsync(Guid id, bool includeDetails = true);
        Task<Calendar> CreateCalendarAsync(Calendar calendar);
        Task<Calendar?> UpdateCalendarAsync(Guid id, Calendar updatedCalendar);
        Task<bool> DeleteCalendarAsync(Guid id);
        Task<bool> CalendarExistsAsync(Guid id);
    }
}
