﻿using FamilyMoviesLibrary.Context.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyMoviesLibrary.Context;

public class FamilyMoviesLibraryContext : DbContext
{
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

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
        /*modelBuilder.Entity<User>()
            .HasOne(x => x.Group)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.GroupId)
            .HasPrincipalKey(x => x.Id);*/
    }
}