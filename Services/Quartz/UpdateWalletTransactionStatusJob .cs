using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Data.DataAccess;
namespace Services.Quartz;

public class UpdateWalletTransactionStatusJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public UpdateWalletTransactionStatusJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var transactions = dbContext.WalletTransactions
                                        .Where(t => t.Status == Data.Enums.WalletTransactionStatus.Waiting)
                                        .ToList();

            foreach (var transaction in transactions)
            {
                transaction.Status = Data.Enums.WalletTransactionStatus.Failure;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
