using Common.Helpers;
using Core.Interfaces;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class HeaderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Rating { get; set; }
    }
}
