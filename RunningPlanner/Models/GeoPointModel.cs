namespace RunningPlanner.Models
{
    public class GeoPoint
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double MaxDistanceMeters { get; set; } = 5000; // Default to 5 km
    }
}