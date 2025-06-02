using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;

public class ServiceProviderMock : IServiceProvider
{
    private readonly RunningPlannerDbContext _context;
    public ServiceProviderMock(RunningPlannerDbContext context) => _context = context;
    public object? GetService(Type serviceType) => serviceType == typeof(RunningPlannerDbContext) ? _context : null;
}

public static class DbContextFactory
{
    public static RunningPlannerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<RunningPlannerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RunningPlannerDbContext(options);
    }
}
