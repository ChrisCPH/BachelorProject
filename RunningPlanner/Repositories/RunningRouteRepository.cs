using MongoDB.Driver;
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
    }
    public class RunningRouteRepository : IRunningRouteRepository
    {
        private readonly IMongoCollection<RunningRoute> _runningRoutes;

        public RunningRouteRepository(IMongoDatabase database)
        {
            _runningRoutes = database.GetCollection<RunningRoute>("RunningRoutes");
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
    }
}