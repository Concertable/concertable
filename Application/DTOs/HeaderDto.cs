namespace Application.DTOs
{
    public class HeaderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public double? Rating { get; set; }
    }
}
