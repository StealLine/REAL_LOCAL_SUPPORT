using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Support_Bot.DB_METHODS.ContextAndHelpers
{
    public class ContextForDbFactory : IDesignTimeDbContextFactory<ContextForDB>
    {
        public ContextForDB CreateDbContext(string[] args)
        {
            DotNetEnv.Env.Load();
            var options = new DbContextOptionsBuilder<ContextForDB>();
            var conn = Environment.GetEnvironmentVariable("DBconnection");
            options.UseNpgsql(conn);
            return new ContextForDB(options.Options);
        }
    }
}
