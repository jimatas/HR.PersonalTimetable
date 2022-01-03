using HR.PersonalCalendar.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.PersonalCalendar.Api.Persistence.Configurations
{
    internal class PersonalTimetableConfiguration : IEntityTypeConfiguration<PersonalTimetable>
    {
        public void Configure(EntityTypeBuilder<PersonalTimetable> builder)
        {
            builder.Property(table => table.DateCreated).HasDefaultValueSql("sysdatetimeoffset()");
            builder.Property(table => table.ElementName).HasMaxLength(100).IsRequired();
            builder.Property(table => table.ElementType);
            builder.Property(table => table.InstituteName).HasMaxLength(50).IsRequired();
            builder.Property(table => table.UserName).HasMaxLength(25).IsRequired();
            builder.HasIndex(table => table.UserName).HasDatabaseName("IX_PersonalTimetable_UserName");
        }
    }
}
