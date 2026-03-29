using Concertable.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Models;

public class ConcertApplicationWithStatus
{
    public required ConcertApplicationEntity Application { get; set; }
    public bool HasConcert { get; set; }
}

