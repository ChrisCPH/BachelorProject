using RunningPlanner.Data;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Repositories
{
    public interface IRunRepository
    {
        Task<Run> AddRunAsync(Run run);
        Task<List<Run>> AddRunsAsync(List<Run> runs);
        Task<Run?> GetRunByIdAsync(int runId);
        Task<List<Run>?> GetAllRunsByTrainingPlanAsync(int trainingPlanId);
        Task<Run> UpdateRunAsync(Run run);
        Task<bool> DeleteRunAsync(int runId);
        Task RemoveRouteIdFromRunsAsync(string routeId);
    }

    public class RunRepository : IRunRepository
    {
        private readonly RunningPlannerDbContext _context;

        public RunRepository(RunningPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<Run> AddRunAsync(Run run)
        {
            await _context.Run.AddAsync(run);
            await _context.SaveChangesAsync();
            return run;
        }

        public async Task<List<Run>> AddRunsAsync(List<Run> runs)
        {
            await _context.Run.AddRangeAsync(runs);
            await _context.SaveChangesAsync();
            return runs;
        }

        public async Task<Run?> GetRunByIdAsync(int runId)
        {
            return await _context.Run.FindAsync(runId);
        }

        public async Task<List<Run>?> GetAllRunsByTrainingPlanAsync(int trainingPlanId)
        {
            var trainingPlan = await _context.TrainingPlan
                .Include(tp => tp.Runs)
                .FirstOrDefaultAsync(tp => tp.TrainingPlanID == trainingPlanId);

            if (trainingPlan == null) return null;

            return trainingPlan.Runs;
        }

        public async Task<Run> UpdateRunAsync(Run run)
        {
            _context.Run.Update(run);
            await _context.SaveChangesAsync();
            return run;
        }

        public async Task<bool> DeleteRunAsync(int runId)
        {
            var run = await _context.Run.FindAsync(runId);
            if (run == null) return false;

            _context.Run.Remove(run);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task RemoveRouteIdFromRunsAsync(string routeId)
        {
            var runsWithRoute = await _context.Run.Where(r => r.RouteID == routeId).ToListAsync();

            foreach (var run in runsWithRoute)
            {
                run.RouteID = null;
            }

            await _context.SaveChangesAsync();
        }
    }
}