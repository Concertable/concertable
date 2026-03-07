namespace Application.Requests
{
    public record UpdateLocationRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
