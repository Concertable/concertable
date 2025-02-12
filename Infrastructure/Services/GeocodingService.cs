using Application.DTOs;
using Application.Interfaces;
using Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient httpClient;
        private readonly string apiKey;

        public GeocodingService(HttpClient httpClient, IConfiguration configuration) 
        {
            this.httpClient = httpClient;
            apiKey = configuration["GoogleApiKey"];
        }

        public async Task<LocationDto> GetLocationAsync(CoordinatesDto coordinatesDto)
        {
            var latLng = $"{coordinatesDto.Latitude},{coordinatesDto.Longitude}";
            var query = $"latlng={Uri.EscapeDataString(latLng)}&key={Uri.EscapeDataString(apiKey)}"; //Adds latlong to url safely
            Uri requestUri = new Uri(httpClient.BaseAddress, $"json?{query}");

            //Send to api
            var response = await httpClient.GetStringAsync(requestUri);
            //Deserialize response
            dynamic result = JsonConvert.DeserializeObject(response);

            string county = null;
            string town = null;

            if (result.results == null || result.results.Count == 0)
                throw new BadRequestException("No geocoding results found for the provided coordinates.");

            foreach (var resultItem in result.results)
            {
                foreach (var addressComponent in resultItem.address_components)
                {
                    var types = addressComponent.types.ToObject<List<string>>();
                    string longName = addressComponent.long_name;

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

            return new LocationDto
            {
                County = county,
                Town = town
            };
        }
    }
}
