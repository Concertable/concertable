using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateArtistDto
    {
        public string Name { get; set; }
        public string About { get; set; }
        public string ImageUrl { get; set; }
        public IEnumerable<string> Genres { get; set; }
    }
}
