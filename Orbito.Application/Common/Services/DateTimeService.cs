using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Common.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
