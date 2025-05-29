using System.Net;
using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface IRunningRouteService
    {
        Task<List<RunningRoute>?> GetAllAsync();
        Task<RunningRoute?> GetByIdAsync(string id);
        Task AddAsync(RunningRoute runningRoute);
        Task UpdateAsync(string id, RunningRoute runningRoute);
        Task DeleteAsync(string id);
        Task<List<RunningRoute>> GetRoutesNearPointAsync(double longitude, double latitude, double maxDistanceMeters);
        Task<List<RunningRoute>> GetRoutesWithinPolygonAsync(Polygon polygon);
        Task<List<RunningRoute>> GetRoutesIntersectingPolygonAsync(List<List<double>> polygonCoordinates);
    }

    public class RunningRouteService : IRunningRouteService
    {
        private readonly IRunningRouteRepository _runningRouteRepository;
        private readonly IRunRepository _runRepository;

        public RunningRouteService(IRunningRouteRepository runningRouteRepository, IRunRepository runRepository)
        {
            _runningRouteRepository = runningRouteRepository;
            _runRepository = runRepository;
        }

        public async Task<List<RunningRoute>?> GetAllAsync()
        {
            return await _runningRouteRepository.GetAllAsync();
        }

        public async Task<RunningRoute?> GetByIdAsync(string id)
        {
            var route = await _runningRouteRepository.GetByIdAsync(id);
            if (route == null)
            {
                throw new KeyNotFoundException($"Route with id {id} not found");
            }
            return route;
        }

        public async Task AddAsync(RunningRoute route)
        {
            route.Name = WebUtility.HtmlEncode(route.Name);

            await _runningRouteRepository.AddAsync(route);
        }

        public async Task UpdateAsync(string id, RunningRoute route)
        {
            var existingRoute = await _runningRouteRepository.GetByIdAsync(id);
            if (existingRoute == null)
            {
                throw new KeyNotFoundException($"Route with id {id} not found");
            }

            route.Name = WebUtility.HtmlEncode(route.Name);
            
            await _runningRouteRepository.UpdateAsync(id, route);
        }

        public async Task DeleteAsync(string id)
        {
            var existingRoute = await _runningRouteRepository.GetByIdAsync(id);
            if (existingRoute == null)
            {
                throw new KeyNotFoundException($"Route with id {id} not found");
            }

            await _runRepository.RemoveRouteIdFromRunsAsync(id);

            await _runningRouteRepository.DeleteAsync(id);
        }

        public async Task<List<RunningRoute>> GetRoutesNearPointAsync(double longitude, double latitude, double maxDistanceMeters)
        {
            return await _runningRouteRepository.FindRoutesNearPointAsync(longitude, latitude, maxDistanceMeters);
        }

        public async Task<List<RunningRoute>> GetRoutesWithinPolygonAsync(Polygon polygon)
        {
            return await _runningRouteRepository.GetRoutesWithinPolygonAsync(polygon.Coordinates);
        }

        public async Task<List<RunningRoute>> GetRoutesIntersectingPolygonAsync(List<List<double>> polygonCoordinates)
        {
            return await _runningRouteRepository.GetRoutesGeoIntersectsAsync(polygonCoordinates);
        }
    }
}