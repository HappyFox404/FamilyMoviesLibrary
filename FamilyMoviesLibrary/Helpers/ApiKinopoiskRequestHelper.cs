using System.Net;
using FamilyMoviesLibrary.Models.Data;
using FamilyMoviesLibrary.Models.Exception;
using FamilyMoviesLibrary.Services;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace FamilyMoviesLibrary.Helpers;

public class ApiKinopoiskRequestHelper
{
    private readonly RestClient _client;
    
    public ApiKinopoiskRequestHelper()
    {
        _client = new RestClient(SettingsService.GetUnofficalKinopoiskUrl());
    }

    public async Task<List<FilmKinopoiskUnofficalModel>> SearchFilm(string name)
    {
        try
        {
            List<FilmKinopoiskUnofficalModel> result = new();
            string token = SettingsService.GetUnofficalKinopoiskToken();
            var request = new RestRequest(SettingsService.GetUnofficalKinopoiskFilmsMethod())
                .AddHeader("X-API-KEY", token)
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
                return result.Where(x => x.Year != default).ToList();
            }
            throw new ControllException("Ошибка прии получении данных фильмов.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error load genres: {ex}", ex.Message);
            throw;
        }
    }
}