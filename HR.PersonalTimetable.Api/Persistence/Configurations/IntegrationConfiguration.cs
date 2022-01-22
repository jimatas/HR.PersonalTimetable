using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.PersonalTimetable.Api.Persistence.Configurations
{
    public class IntegrationConfiguration: IEntityTypeConfiguration<Models.Integration>
    {
        public void Configure(EntityTypeBuilder<Models.Integration> builder)
        {
            builder.ToTable("Integrations");
            builder.Property(table => table.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(table => table.Name).IsUnique().HasDatabaseName("IX_Integrations_Name");
        }
    }
}
