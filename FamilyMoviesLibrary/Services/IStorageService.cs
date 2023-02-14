using System.Net;
using FamilyMoviesLibrary.Context.Migrations;
using FamilyMoviesLibrary.Models.Data;
using FamilyMoviesLibrary.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace FamilyMoviesLibrary.Services;

public interface IStorageService
{
    IEnumerable<GenresKinopoiskUnofficalModel> GetGenres();
    IEnumerable<CountriesKinopoiskUnofficalModel> GetCountries();
}

public class StorageService : IStorageService
{
    private List<GenresKinopoiskUnofficalModel> _genres = new List<GenresKinopoiskUnofficalModel>();
    private List<CountriesKinopoiskUnofficalModel> _countries = new List<CountriesKinopoiskUnofficalModel>();

    private readonly KinpoiskUnofficalSettings _settings;
    private readonly ILogger<StorageService> _logger;
    public StorageService(IOptions<KinpoiskUnofficalSettings> options, ILogger<StorageService> logger)
    {
        _logger = logger;
        _settings = options.Value;
        LoadKinopoiskUnofficalFilterData();
    }
    
    private void LoadKinopoiskUnofficalFilterData()
    {
        if (_genres.Any() == false || _countries.Any() == false)
        {
            try
            {
                string token = _settings.Token;
                var client = new RestClient(_settings.KinopoiskUnofficalUrl);

                var request = new RestRequest(_settings.Genres)
                    .AddHeader("X-API-KEY", token)
                    .AddHeader("Content-Type", "application/json");

                var response = client.Get(request);
                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    JObject responseData = JObject.Parse(response.Content);
                    _genres = (responseData.SelectToken("genres") ?? throw new InvalidOperationException())
                        .Select(s => s.ToObject<GenresKinopoiskUnofficalModel>()).ToList();
                    _countries = (responseData.SelectToken("countries") ?? throw new InvalidOperationException())
                        .Select(s => s.ToObject<CountriesKinopoiskUnofficalModel>()).ToList();

                    if (_genres.Any() == false)
                        throw new ArgumentNullException("Нет данных о жанрах");
                    if (_countries.Any() == false)
                        throw new ArgumentNullException("Нет данных о странах");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error load genres: {ex}", ex.Message);
                throw;
            }
        }
    }

    public IEnumerable<GenresKinopoiskUnofficalModel> GetGenres()
    {
        return _genres;
    }

    public IEnumerable<CountriesKinopoiskUnofficalModel> GetCountries()
    {
        return _countries;
    }
}