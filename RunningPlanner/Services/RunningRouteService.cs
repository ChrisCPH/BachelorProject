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
    }

    public class RunningRouteService : IRunningRouteService
    {
        private readonly IRunningRouteRepository _runningRouteRepository;

        public RunningRouteService(IRunningRouteRepository runningRouteRepository)
        {
            _runningRouteRepository = runningRouteRepository;
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
            await _runningRouteRepository.AddAsync(route);
        }

        public async Task UpdateAsync(string id, RunningRoute route)
        {
            var existingRoute = await _runningRouteRepository.GetByIdAsync(id);
            if (existingRoute == null)
            {
                throw new KeyNotFoundException($"Route with id {id} not found");
            }
            await _runningRouteRepository.UpdateAsync(id, route);
        }

        public async Task DeleteAsync(string id)
        {
            var existingRoute = await _runningRouteRepository.GetByIdAsync(id);
            if (existingRoute == null)
            {
                throw new KeyNotFoundException($"Route with id {id} not found");
            }
            await _runningRouteRepository.DeleteAsync(id);
        }
    }
}