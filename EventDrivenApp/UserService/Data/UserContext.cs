using Microsoft.EntityFrameworkCore;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Thomas Peters", Email = "tpeters@ttlabs.co.za" },
            new User { Id = 2, Name = "Piet", Email = "jpiet@gmail.com" },
            new User { Id = 3, Name = "Joe", Email = "Joe@gmail.com" },
            new User { Id = 4, Name = "Caleb", Email = "Caleb@gmail.com" }
        );
    }
}