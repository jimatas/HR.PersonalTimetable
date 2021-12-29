using HR.PersonalCalendar.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.PersonalCalendar.Persistence.Configurations
{
    internal class PersonalTimetableConfiguration : IEntityTypeConfiguration<PersonalTimetable>
    {
        public void Configure(EntityTypeBuilder<PersonalTimetable> builder)
        {
            builder.Property(t => t.DateCreated).HasDefaultValueSql("sysdatetimeoffset()");
            builder.Property(t => t.ElementName).HasMaxLength(100).IsRequired();
            builder.Property(t => t.ElementType);
            builder.Property(t => t.InstituteName).HasMaxLength(50).IsRequired();
            builder.Property(t => t.UserName).HasMaxLength(25).IsRequired();
            builder.HasIndex(t => t.UserName).HasDatabaseName("IX_PersonalTimetable_UserName");
        }
    }
}
