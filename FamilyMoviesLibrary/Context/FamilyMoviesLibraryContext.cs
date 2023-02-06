using FamilyMoviesLibrary.Context.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyMoviesLibrary.Context;

public class FamilyMoviesLibraryContext : DbContext
{
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Film> Films { get; set; } = null!;

    public FamilyMoviesLibraryContext(DbContextOptions<FamilyMoviesLibraryContext> options) : base(options)
    {
        //Database.EnsureCreated();
    }

    public static FamilyMoviesLibraryContext CreateContext(string connection)
    {
        DbContextOptionsBuilder<FamilyMoviesLibraryContext> options = new();
        options.UseNpgsql(connection);
        return new FamilyMoviesLibraryContext(options.Options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>().HasKey(x => x.Id);
        modelBuilder.Entity<Group>().Property(x => x.Name).IsRequired();
        modelBuilder.Entity<Group>().HasIndex(x => x.Name).IsUnique();
        
        modelBuilder.Entity<User>().HasKey(x => x.Id);
        modelBuilder.Entity<User>().Property(x => x.TelegramId).IsRequired();

        modelBuilder.Entity<Message>().HasKey(x => x.Id);
        modelBuilder.Entity<Message>().Property(x => x.IsNeedAdditionalMessage).IsRequired();
        modelBuilder.Entity<User>()
            .HasOne(x => x.Message)
            .WithOne(x => x.User)
            .HasForeignKey<Message>(x => x.UserId)
            .HasPrincipalKey<User>(x => x.Id);

        modelBuilder.Entity<Film>().HasKey(x => x.Id);
        modelBuilder.Entity<Film>().Property(x => x.KinopoiskId).IsRequired();
        modelBuilder.Entity<Film>().Property(x => x.Rate).IsRequired();
        modelBuilder.Entity<Film>().Property(x => x.GroupId).IsRequired();
    }
}