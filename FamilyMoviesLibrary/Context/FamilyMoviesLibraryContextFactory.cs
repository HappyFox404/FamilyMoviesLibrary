using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FamilyMoviesLibrary.Context;

public class FamilyMoviesLibraryContextFactory : IDesignTimeDbContextFactory<FamilyMoviesLibraryContext>
{
    public FamilyMoviesLibraryContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FamilyMoviesLibraryContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=LibraryMovies;Username=admin;Password=12345rvs");
        return new FamilyMoviesLibraryContext(optionsBuilder.Options);
    }
}