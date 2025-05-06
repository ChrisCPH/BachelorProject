using MongoDB.Bson.Serialization.Attributes;

namespace RunningPlanner.Models
{
    public class GeoJsonLineString
    {
        [BsonElement("type")]
        public string Type { get; set; } = "LineString";

        [BsonElement("coordinates")]
        public List<List<double>> Coordinates { get; set; } = new();
    }
}