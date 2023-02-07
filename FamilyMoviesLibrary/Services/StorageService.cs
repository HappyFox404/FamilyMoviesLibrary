using System.Net;
using FamilyMoviesLibrary.Models.Data;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace FamilyMoviesLibrary.Services;

public static class StorageService
{
    public static List<GenresKinopoiskUnofficalModel> Genres = new List<GenresKinopoiskUnofficalModel>();
    public static List<CountriesKinopoiskUnofficalModel> Countries = new List<CountriesKinopoiskUnofficalModel>();

    public static void LoadKinopoiskUnofficalFilterData()
    {
        if (Genres.Any() == false || Countries.Any() == false)
        {
            try
            {
                string token = SettingsService.GetUnofficalKinopoiskToken();
                var client = new RestClient(SettingsService.GetUnofficalKinopoiskUrl());

                var request = new RestRequest(SettingsService.GetUnofficalKinopoiskGenresMethod())
                    .AddHeader("X-API-KEY", token)
                    .AddHeader("Content-Type", "application/json");

                var response = client.Get(request);
                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    JObject responseData = JObject.Parse(response.Content);
                    Genres = (responseData.SelectToken("genres") ?? throw new InvalidOperationException())
                        .Select(s => s.ToObject<GenresKinopoiskUnofficalModel>()).ToList();
                    Countries = (responseData.SelectToken("countries") ?? throw new InvalidOperationException())
                        .Select(s => s.ToObject<CountriesKinopoiskUnofficalModel>()).ToList();

                    if (Genres.Any() == false)
                        throw new ArgumentNullException("Нет данных о жанрах");
                    if (Countries.Any() == false)
                        throw new ArgumentNullException("Нет данных о странах");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error load genres: {ex}", ex.Message);
                throw;
            }
        }
    }
}