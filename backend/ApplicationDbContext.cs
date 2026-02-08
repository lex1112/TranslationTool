using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using translation_app.Model;
using translation_app.Models;

namespace translation_app
{
    public class ApplicationDbContext : IdentityDbContext
    {
        // Table for unique SIDs
        public DbSet<TextResource> TextResources { get; set; }

        // Table for the actual translated strings
        public DbSet<Translation> Translations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Essential for Identity tables (AspNetUsers, etc.)
            base.OnModelCreating(modelBuilder);

            // 1. Configure TextResource (The Parent)
            modelBuilder.Entity<TextResource>()
                .HasKey(r => r.Sid);

            // 2. Configure Translation (The Child)
            modelBuilder.Entity<Translation>()
                .HasKey(t => t.Id); // Internal ID for easier DB management

            // 3. Define the Relationship (One SID -> Many Translations)
            modelBuilder.Entity<Translation>()
                .HasOne(t => t.TextResource)
                .WithMany(r => r.Translations)
                .HasForeignKey(t => t.Sid)
                .OnDelete(DeleteBehavior.Cascade); // Requirement: Delete SID -> Delete all translations

            // 4. Ensure Unique constraint: One SID cannot have two "en" translations
            modelBuilder.Entity<Translation>()
                .HasIndex(t => new { t.Sid, t.LangId })
                .IsUnique();
        }
    }
}