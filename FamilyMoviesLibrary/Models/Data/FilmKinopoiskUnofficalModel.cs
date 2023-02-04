namespace FamilyMoviesLibrary.Models.Data;

public class FilmKinopoiskUnofficalModel
{
    public long KinopoiskId { get; set; }
    public string? ImdbId { get; set; }
    public string? NameRu { get; set; }
    public string? NameEn { get; set; }
    public string? NameOriginal { get; set; }
    public List<FilmCountryKinopoiskUnofficalModel?> Countries { get; set; } = new();
    public List<FilmGenreKinopoiskUnofficalModel?> Genres { get; set; } = new();
    public float? RatingKinopoisk { get; set; }
    public float? RatingImdb { get; set; }
    public int? Year { get; set; }
    public string? Type { get; set; }
    public string? PosterUrl { get; set; }
    public string? PosterUrlPreview { get; set; }
}

public class FilmCountryKinopoiskUnofficalModel{
    public string? Country { get; set; }
}

public class FilmGenreKinopoiskUnofficalModel{
    public string? Genre { get; set; }
}