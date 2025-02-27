using Application.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ListingResponseDto
    {
        public int? Id { get; set; }
        public DateTime StartDate { get; set; }
        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly EndTime { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; }
        public double Pay { get; set; }
    }
}
