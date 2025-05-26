using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface IRunService
    {
        Task<Run> CreateRunAsync(Run run);
        Task<List<Run>> CreateRepeatedRunAsync(Run run);
        Task<Run?> GetRunByIdAsync(int runId);
        Task<List<Run>?> GetAllRunsByTrainingPlanAsync(int trainingPlanId);
        Task<Run> UpdateRunAsync(Run run);
        Task<bool> DeleteRunAsync(int runId);
    }

    public class RunService : IRunService
    {
        private readonly IRunRepository _runRepository;
        private readonly ITrainingPlanRepository _trainingPlanRepository;

        public RunService(IRunRepository runRepository, ITrainingPlanRepository trainingPlanRepository)
        {
            _trainingPlanRepository = trainingPlanRepository;
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

        public async Task<List<Run>> CreateRepeatedRunAsync(Run run)
        {
            if (run == null)
                throw new ArgumentNullException(nameof(run));

            var trainingPlan = await _trainingPlanRepository.GetTrainingPlanByIdAsync(run.TrainingPlanID);

            if (trainingPlan == null)
                throw new ArgumentException("Training plan not found");

            var runsToCreate = new List<Run>();

            var now = DateTime.UtcNow; // Supposed to help with speed instead of doing it in the loop

            for (int week = 1; week <= trainingPlan.Duration; week++)
            {
                var runForWeek = new Run
                {
                    TrainingPlanID = run.TrainingPlanID,
                    Type = run.Type,
                    WeekNumber = week,
                    DayOfWeek = run.DayOfWeek,
                    TimeOfDay = run.TimeOfDay,
                    Distance = run.Distance,
                    Duration = run.Duration,
                    Pace = run.Pace,
                    Notes = run.Notes,
                    Completed = run.Completed,
                    CreatedAt = now,
                };

                runsToCreate.Add(runForWeek);
            }

            var createdRuns = await _runRepository.AddRunsAsync(runsToCreate);

            return createdRuns;
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