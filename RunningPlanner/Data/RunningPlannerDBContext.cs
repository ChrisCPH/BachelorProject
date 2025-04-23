using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Data
{
    public class RunningPlannerDbContext : DbContext
    {
        public RunningPlannerDbContext(DbContextOptions<RunningPlannerDbContext> options) : base(options) { }
    
        public DbSet<User> User { get; set; } = null!;
        public DbSet<TrainingPlan> TrainingPlan { get; set; } = null!;
        public DbSet<Run> Run { get; set; } = null!;
        public DbSet<Workout> Workout { get; set; } = null!;
    }
}
