using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface IRunService
    {
        Task<Run> CreateRunAsync(Run run);
        Task<Run?> GetRunByIdAsync(int runId);
        Task<List<Run>?> GetAllRunsByTrainingPlanAsync(int trainingPlanId);
        Task<Run> UpdateRunAsync(Run run);
        Task<bool> DeleteRunAsync(int runId);
    }

    public class RunService : IRunService
    {
        private readonly IRunRepository _runRepository;

        public RunService(IRunRepository runRepository)
        {
            _runRepository = runRepository;
        }

        public async Task<Run> CreateRunAsync(Run run)
        {
            if (run == null)
            {
                throw new ArgumentNullException(nameof(run), "Run data is required.");
            }

            return await _runRepository.AddRunAsync(run);
        }

        public async Task<Run?> GetRunByIdAsync(int runId)
        {
            return await _runRepository.GetRunByIdAsync(runId);
        }

        public async Task<List<Run>?> GetAllRunsByTrainingPlanAsync(int trainingPlanId)
        {
            return await _runRepository.GetAllRunsByTrainingPlanAsync(trainingPlanId);
        }

        public async Task<Run> UpdateRunAsync(Run run)
        {
            if (run == null)
            {
                throw new ArgumentNullException(nameof(run), "Run data is required.");
            }

            return await _runRepository.UpdateRunAsync(run);
        }

        public async Task<bool> DeleteRunAsync(int runId)
        {
            return await _runRepository.DeleteRunAsync(runId);
        }
    }
}