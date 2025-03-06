using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreatePreferenceDto
    {
        public int RadiusKm { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; }
    }
}
