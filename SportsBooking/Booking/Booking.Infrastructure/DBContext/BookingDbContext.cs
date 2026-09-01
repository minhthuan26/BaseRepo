using Microsoft.EntityFrameworkCore;
using CommonLibs.BaseDbContext;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.DBContext
{
    public class BookingDbContext : BaseContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options, ILogger<BookingDbContext> logger) : base(options, logger)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
