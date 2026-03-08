using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models;

public class ListingApplicationWithStatus
{
    public required ListingApplication Application { get; set; }
    public bool HasConcert { get; set; }
}

