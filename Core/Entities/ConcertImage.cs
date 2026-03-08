

namespace Core.Entities
{
    public class ConcertImage : BaseEntity
    {
        public int ConcertId { get; set; }
        public int Type { get; set; }
        public string Url { get; set; }
        public Concert Concert { get; set; }


    }
}
