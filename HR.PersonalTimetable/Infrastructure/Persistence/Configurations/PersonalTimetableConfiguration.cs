using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.PersonalTimetable.Infrastructure.Persistence.Configurations
{
    internal class PersonalTimetableConfiguration : IEntityTypeConfiguration<Application.Models.PersonalTimetable>
    {
        public void Configure(EntityTypeBuilder<Application.Models.PersonalTimetable> builder)
        {
            builder.ToTable("PersonalTimetables");
            builder.Property(table => table.DateCreated).HasDefaultValueSql("sysdatetimeoffset()");
            builder.Property(table => table.ElementName).HasMaxLength(100).IsRequired();
            builder.Property(table => table.ElementType);
            builder.Property(table => table.InstituteName).HasMaxLength(50).IsRequired();
            builder.Property(table => table.UserName).HasMaxLength(25).IsRequired();
            builder.HasIndex(table => table.UserName).HasDatabaseName("IX_PersonalTimetables_UserName");
            builder.HasOne(table => table.Integration).WithMany().HasForeignKey("IntegrationId").IsRequired();
        }
    }
}
