using Microsoft.EntityFrameworkCore;
using Support_Bot.DB_METHODS.Config;
using Support_Bot.DB_METHODS.Entitys;

namespace Support_Bot.DB_METHODS.ContextAndHelpers
{
    public class ContextForDB : DbContext
    {
        public ContextForDB(DbContextOptions<ContextForDB> options) : base(options) { }
        public DbSet<TicketLogs> TicketLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TicketLogsConfiguration());
        }

    }
}
