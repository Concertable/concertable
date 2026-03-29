using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Core.Interfaces;

public interface IHasLocation
{
    Point? Location { get; }
}
