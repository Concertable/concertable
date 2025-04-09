using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Preference : BaseEntity
    {
        public int UserId { get; set; }
        public double RadiusKm  { get; set; }
        public Customer User { get; set; }
        public ICollection<GenrePreference> GenrePreferences { get; set; } = new List<GenrePreference>();
    }
}
