using HelpdeskSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSystem.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Ticket>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<Ticket>().Property(x => x.Priority).HasConversion<string>();
        modelBuilder.Entity<User>().Property(x => x.Role).HasConversion<string>();
        modelBuilder.Entity<Ticket>().HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Ticket>().HasOne(x => x.AssignedTo).WithMany().HasForeignKey(x => x.AssignedToId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Comment>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Comment>().HasOne(x => x.Ticket).WithMany(x => x.Comments).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Donanım" },
            new Category { Id = 2, Name = "Yazılım" },
            new Category { Id = 3, Name = "Ağ ve İnternet" },
            new Category { Id = 4, Name = "Kullanıcı Hesabı" });
    }
}
