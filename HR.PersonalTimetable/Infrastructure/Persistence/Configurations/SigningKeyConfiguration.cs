using HR.PersonalTimetable.Application.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.PersonalTimetable.Infrastructure.Persistence.Configurations
{
    public class SigningKeyConfiguration : IEntityTypeConfiguration<SigningKey>
    {
        public void Configure(EntityTypeBuilder<SigningKey> builder)
        {
            builder.ToTable("SigningKeys");
            builder.Property(table => table.DateCreated).HasDefaultValueSql("sysdatetimeoffset()");
            builder.Property(table => table.Key).HasColumnName("SigningKey").HasMaxLength(255).IsRequired();
            builder.HasOne(table => table.Integration).WithMany(table => table.SigningKeys).HasForeignKey("IntegrationId").IsRequired();
        }
    }
}
