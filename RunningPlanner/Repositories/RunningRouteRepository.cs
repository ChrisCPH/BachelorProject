using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using RunningPlanner.Models;

namespace RunningPlanner.Repositories
{
    public interface IRunningRouteRepository
    {
        Task<List<RunningRoute>?> GetAllAsync();
        Task<RunningRoute?> GetByIdAsync(string id);
        Task AddAsync(RunningRoute runningRoute);
        Task UpdateAsync(string id, RunningRoute runningRoute);
        Task DeleteAsync(string id);
        Task<List<RunningRoute>> FindRoutesNearPointAsync(double longitude, double latitude, double maxDistanceMeters);
        Task<List<RunningRoute>> GetRoutesWithinPolygonAsync(List<List<double>> rawCoordinates);
        Task<List<RunningRoute>> GetRoutesGeoIntersectsAsync(List<List<double>> rawCoordinates);
    }
    public class RunningRouteRepository : IRunningRouteRepository
    {
        private readonly IMongoCollection<RunningRoute> _runningRoutes;

        public RunningRouteRepository(IMongoDatabase database)
        {
            _runningRoutes = database.GetCollection<RunningRoute>("RunningRoutes");

            var keys = Builders<RunningRoute>.IndexKeys.Geo2DSphere("geometry");
            var indexModel = new CreateIndexModel<RunningRoute>(keys);
            _runningRoutes.Indexes.CreateOne(indexModel);
        }

        public async Task<List<RunningRoute>?> GetAllAsync()
        {
            return await _runningRoutes.Find(_ => true).ToListAsync();
        }

        public async Task<RunningRoute?> GetByIdAsync(string id)
        {
            return await _runningRoutes.Find(runningRoute => runningRoute.ID == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(RunningRoute runningRoute)
        {
            await _runningRoutes.InsertOneAsync(runningRoute);
        }

        public async Task UpdateAsync(string id, RunningRoute runningRoute)
        {
            var filter = Builders<RunningRoute>.Filter.Eq(r => r.ID, id);
            await _runningRoutes.ReplaceOneAsync(filter, runningRoute);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<RunningRoute>.Filter.Eq(r => r.ID, id);
            await _runningRoutes.DeleteOneAsync(filter);
        }

        public async Task<List<RunningRoute>> FindRoutesNearPointAsync(double longitude, double latitude, double maxDistanceMeters)
        {
            var point = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(longitude, latitude));

            var filter = Builders<RunningRoute>.Filter.NearSphere(r => r.Geometry, point, maxDistanceMeters);

            return await _runningRoutes.Find(filter).ToListAsync();
        }

        public async Task<List<RunningRoute>> GetRoutesWithinPolygonAsync(List<List<double>> rawCoordinates)
        {
            if (!rawCoordinates.First().SequenceEqual(rawCoordinates.Last()))
            {
                rawCoordinates.Add(rawCoordinates.First());
            }

            var coordinates = rawCoordinates
                .Select(coord => new GeoJson2DCoordinates(coord[0], coord[1]))
                .ToList();

            var ring = new GeoJsonLinearRingCoordinates<GeoJson2DCoordinates>(coordinates);
            var polygonCoordinates = new GeoJsonPolygonCoordinates<GeoJson2DCoordinates>(ring);
            var polygon = new GeoJsonPolygon<GeoJson2DCoordinates>(polygonCoordinates);

            var filter = Builders<RunningRoute>.Filter.GeoWithin(r => r.Geometry, polygon);

            return await _runningRoutes.Find(filter).ToListAsync();
        }

        public async Task<List<RunningRoute>> GetRoutesGeoIntersectsAsync(List<List<double>> rawCoordinates)
        {
            if (!rawCoordinates.First().SequenceEqual(rawCoordinates.Last()))
            {
                rawCoordinates.Add(rawCoordinates.First());
            }

            var coordinates = rawCoordinates
                .Select(coord => new GeoJson2DCoordinates(coord[0], coord[1]))
                .ToList();

            var ring = new GeoJsonLinearRingCoordinates<GeoJson2DCoordinates>(coordinates);
            var polygonCoordinates = new GeoJsonPolygonCoordinates<GeoJson2DCoordinates>(ring);
            var polygon = new GeoJsonPolygon<GeoJson2DCoordinates>(polygonCoordinates);

            var filter = Builders<RunningRoute>.Filter.GeoIntersects(r => r.Geometry, polygon);

            return await _runningRoutes.Find(filter).ToListAsync();
        }
    }
}