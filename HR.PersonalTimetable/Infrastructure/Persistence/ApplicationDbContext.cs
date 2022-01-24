using Microsoft.EntityFrameworkCore;

namespace HR.PersonalTimetable.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private const string DefaultConnectionString = "Server=(localdb)\\mssqllocaldb;Database=dev_Playground;Trusted_Connection=true;MultipleActiveResultSets=true";

        public ApplicationDbContext() { }
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            if (!builder.IsConfigured)
            {
                builder.UseSqlServer(DefaultConnectionString);
            }
            base.OnConfiguring(builder);
        }
    }
}
