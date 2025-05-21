using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;

public abstract class TestBase
{
    protected RunningPlannerDbContext GetInMemoryDbContext(bool useUniqueName = true)
    {
        var options = new DbContextOptionsBuilder<RunningPlannerDbContext>()
            .UseInMemoryDatabase(databaseName: useUniqueName ? Guid.NewGuid().ToString() : "UnitTestDb")
            .Options;

        var context = new RunningPlannerDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}