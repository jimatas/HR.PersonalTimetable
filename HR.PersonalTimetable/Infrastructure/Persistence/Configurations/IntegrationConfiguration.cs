using HR.PersonalTimetable.Application.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.PersonalTimetable.Infrastructure.Persistence.Configurations
{
    internal class IntegrationConfiguration : IEntityTypeConfiguration<Integration>
    {
        public void Configure(EntityTypeBuilder<Integration> builder)
        {
            builder.ToTable("Integrations");
            builder.Property(table => table.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(table => table.Name).IsUnique().HasDatabaseName("IX_Integrations_Name");
        }
    }
}
