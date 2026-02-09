using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translation.Domain.Entities;

namespace Translation.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<TextResourceEntity> TextResources => Set<TextResourceEntity>();
        public DbSet<TranslationEntity> Translations => Set<TranslationEntity>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Setup Identity tables first
            base.OnModelCreating(modelBuilder);

            // 2. Apply DDD Configurations defined below
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }


    public class TextResourceConfiguration : IEntityTypeConfiguration<TextResourceEntity>
    {
        public void Configure(EntityTypeBuilder<TextResourceEntity> builder)
        {
            builder.ToTable("text_resources");

            // Guid technical Primary Key
            builder.HasKey(x => x.Id);

            // Business Unique Key (SID)
            builder.Property(x => x.Sid)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(x => x.Sid).IsUnique();

            // One-to-Many Relationship via SID (String-to-String link)
            // We configure it from here because TranslationEntity has no navigation property back
            builder.HasMany(r => r.Translations)
                .WithOne()
                .HasForeignKey(t => t.Sid)      // Prop in TranslationEntity
                .HasPrincipalKey(r => r.Sid)    // Prop in TextResourceEntity
                .OnDelete(DeleteBehavior.Cascade);

            // DDD: Tell EF to use the private field '_translations' for the collection
            builder.Metadata.FindNavigation(nameof(TextResourceEntity.Translations))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    public class TranslationConfiguration : IEntityTypeConfiguration<TranslationEntity>
    {
        public void Configure(EntityTypeBuilder<TranslationEntity> builder)
        {
            builder.ToTable("translations");

            // Technical Primary Key
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Sid).IsRequired().HasMaxLength(255);
            builder.Property(x => x.LangId).IsRequired().HasMaxLength(10);
            builder.Property(x => x.Text).IsRequired();

            // Business Constraint: Prevent duplicate languages for the same SID
            builder.HasIndex(t => new { t.Sid, t.LangId }).IsUnique();
        }
    }


}