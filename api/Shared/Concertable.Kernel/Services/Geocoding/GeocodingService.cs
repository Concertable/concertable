using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Concertable.Shared.Infrastructure.Services.Geocoding;

public class GeocodingService : IGeocodingService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string apiKey;

    public GeocodingService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        this.httpClientFactory = httpClientFactory;
        apiKey = configuration["GoogleApiKey"]!;
    }

    public async Task<LocationDto> GetLocationAsync(double latitude, double longitude)
    {
        var httpClient = httpClientFactory.CreateClient("Geocoding");
        var latLng = $"{latitude},{longitude}";
        var query = $"latlng={Uri.EscapeDataString(latLng)}&key={Uri.EscapeDataString(apiKey)}";
        var requestUri = new Uri(httpClient.BaseAddress!, $"json?{query}");

        var response = await httpClient.GetStringAsync(requestUri);
        var result = JsonConvert.DeserializeObject<GoogleGeocodeResponse>(response);

        string? county = null;
        string? town = null;

        if (result?.Results == null || result.Results.Count == 0)
            throw new BadRequestException("No geocoding results found for the provided coordinates.");

        foreach (var resultItem in result.Results)
        {
            foreach (var addressComponent in resultItem.Address_Components)
            {
                var types = addressComponent.Types;
                string longName = addressComponent.Long_Name;

                if (types.Contains("administrative_area_level_2") && string.IsNullOrEmpty(county))
                {
                    county = longName;
                }
                else if (types.Contains("postal_town") && string.IsNullOrEmpty(town))
                {
                    town = longName;
                }
            }

            if (!string.IsNullOrEmpty(county) && !string.IsNullOrEmpty(town))
                break;
        }

        if (county == null || town == null) throw new BadRequestException("County or Town not found");

        return new LocationDto(county, town);
    }
}
