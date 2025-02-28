using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class GenrePreference : BaseEntity
    {
        public int PreferenceId { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
        public Preference Preference { get; set; }

    }
}
