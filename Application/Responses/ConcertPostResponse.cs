using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Responses
{
    public class ConcertPostResponse
    {
        public ConcertDto Concert { get; set; }
        public ConcertHeaderDto ConcertHeader { get; set; }
        /*
         * The userIds that are local to the concert location so can be notified
         * when the concert is posted
         */
        public IEnumerable<int> UserIds { get; set; } 
    }
}
