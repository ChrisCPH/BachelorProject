using RunningPlanner.Data;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Repositories
{
    public interface IRunRepository
    {
        Task<Run> AddRunAsync(Run run);
        Task<Run?> GetRunByIdAsync(int runId);
        Task<List<Run>?> GetAllRunsByTrainingPlanAsync(int trainingPlanId);
        Task<Run> UpdateRunAsync(Run run);
        Task<bool> DeleteRunAsync(int runId);
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
    }
}