using JobTracker.Data.Entities.Login;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="options"></param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        /// <summary>
        /// When modelling the database structure, you can specify many attributes directly on the property
        /// (eg, [Required]), but some require a more explicit configuration (eg, if you want to add an index
        /// to a table, or a unique constraint, etc). Those configuration options are typically set up here.
        /// This method is called automatically when a dbContext instance is created via dependency injection
        /// (in a website, this typically occurs automagically when the web server initializes controller classes
        /// in the background at runtime).
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();
        }
    }
}
