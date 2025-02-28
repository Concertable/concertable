using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Responses
{
    public class EventPostResponse
    {
        public EventDto Event { get; set; }
        /*
         * The userIds that are local to the event location so can be notified
         * when the event is posted
         */
        public IEnumerable<int> UserIds { get; set; } 
    }
}
