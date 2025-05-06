using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RunningPlanner.Models
{
    public class RunningRoute
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; } = null!;

        [BsonElement("userId")]
        public int UserID { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("geometry")]
        public GeoJsonLineString Geometry { get; set; } = new();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("distanceKm")]
        public double? DistanceKm { get; set; }
    }
}