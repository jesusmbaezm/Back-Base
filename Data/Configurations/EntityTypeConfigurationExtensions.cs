using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    public static class EntityTypeConfigurationExtensions
    {
        public static void ConfigureAuditFields<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : AuditableEntity
        {
            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .IsRequired();

            builder.Property(x => x.CreatedByUserId)
                .IsRequired();

            builder.Property(x => x.ModifiedByUserId)
                .IsRequired();
        }
    }
}
