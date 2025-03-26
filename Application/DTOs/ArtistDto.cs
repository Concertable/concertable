using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ArtistDto : ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public string ImageUrl { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; }

        public ArtistDto()
        {
            Type = "artist";
        }
    }
}
