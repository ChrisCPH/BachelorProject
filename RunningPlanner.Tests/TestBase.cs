using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;

public abstract class TestBase
{
    protected RunningPlannerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<RunningPlannerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new RunningPlannerDbContext(options);
    }
}
