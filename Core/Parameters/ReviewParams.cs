using Core.Interfaces;

namespace Core.Parameters
{
    public class ReviewParams : IPageParams
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string Type { get; set; }
    }
}
