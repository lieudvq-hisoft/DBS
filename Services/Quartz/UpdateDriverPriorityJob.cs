using Data.DataAccess;
using Data.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Services.Quartz;

public class UpdateDriverPriorityJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public UpdateDriverPriorityJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var drivers = dbContext.Users
                .Include(_ => _.UserRoles)
                    .ThenInclude(_ => _.Role)
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Driver)
                    && _.DriverStatuses.Any(ds => ds.IsOnline == true)
                    && !_.IsDeleted)
                .ToList();
            var now = DateTime.Now;
            foreach (var driver in drivers)
            {
                if (driver.LastTripTime <= now.AddMinutes(-30))
                {
                    driver.Priority += 1;
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
