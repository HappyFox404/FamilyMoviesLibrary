using System.Net;
using FamilyMoviesLibrary.Models.Data;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace FamilyMoviesLibrary.Services;

public interface IFilmService
{
    Task<IEnumerable<FilmKinopoiskUnofficalModel>> SearchFilm(string name);
    Task<IEnumerable<FilmKinopoiskUnofficalModel>> RecommendFilm(int genre, int page);
}

public class FilmService : IFilmService
{
    private readonly ILogger<FilmService> _logger;
    private readonly RestClient _client;
    private readonly KinpoiskUnofficalSettings _settigns;
    
    public FilmService(ILogger<FilmService> logger, IOptions<KinpoiskUnofficalSettings> settigns)
    {
        _logger = logger;
        _settigns = settigns.Value;
        _client = new RestClient(_settigns.KinopoiskUnofficalUrl);
    }

    public async Task<IEnumerable<FilmKinopoiskUnofficalModel>> SearchFilm(string name)
    {
        try
        {
            List<FilmKinopoiskUnofficalModel> result = new();
            var request = new RestRequest(_settigns.Films)
                .AddHeader("X-API-KEY", _settigns.Token)
                .AddHeader("Content-Type", "application/json")
                .AddParameter("order", "RATING")
                .AddParameter("type", "ALL")
                .AddParameter("ratingFrom", "0")
                .AddParameter("ratingTo", "10")
                .AddParameter("yearFrom", "1000")
                .AddParameter("yearTo", "3000")
                .AddParameter("page", "1")
                .AddParameter("keyword", name);
            
            var response = await _client.GetAsync(request);
            if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
            {
                JObject responseData = JObject.Parse(response.Content);
                result = responseData.SelectToken("items").Select(s => s.ToObject<FilmKinopoiskUnofficalModel>()).ToList();
                return result;
            }
            throw new ControllException("Ошибка прии получении данных фильмов.", false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error load genres: {ex}", ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<FilmKinopoiskUnofficalModel>> RecommendFilm(int genre, int page)
    {
        try
        {
            List<FilmKinopoiskUnofficalModel> result = new();
            var request = new RestRequest(_settigns.Films)
                .AddHeader("X-API-KEY", _settigns.Token)
                .AddHeader("Content-Type", "application/json")
                .AddParameter("order", "RATING")
                .AddParameter("type", "ALL")
                .AddParameter("ratingFrom", "0")
                .AddParameter("ratingTo", "10")
                .AddParameter("yearFrom", "1000")
                .AddParameter("yearTo", "3000")
                .AddParameter("page", page.ToString())
                .AddParameter("genres", genre.ToString());
            
            var response = await _client.GetAsync(request);
            if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
            {
                JObject responseData = JObject.Parse(response.Content);
                result = responseData.SelectToken("items").Select(s => s.ToObject<FilmKinopoiskUnofficalModel>())
                    .ToList();
                return result;
            }

            throw new ControllException("Ошибка прии получении данных фильмов.", false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error load genres: {ex}", ex.Message);
            throw;
        }
    }
}