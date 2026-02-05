using Microsoft.EntityFrameworkCore;
using AuthManagerEnterprise.Models.DomainModels;

namespace AuthManagerEnterprise.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor that passes settings (like connection string) to the base DbContext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // This represents your Users table in MS SQL Server
        public DbSet<User> Users { get; set; }

        // This method is called when the database is being created
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // THE FIRST REQUIREMENT: Configure a Unique Index on the Email column
            // This ensures uniqueness at the storage level, as required.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}