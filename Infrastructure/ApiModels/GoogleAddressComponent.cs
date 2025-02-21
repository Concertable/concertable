using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ApiModels
{
    public class GoogleAddressComponent
    {
        public string Long_Name { get; set; }
        public List<string> Types { get; set; }
    }
}
