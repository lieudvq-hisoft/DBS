using Data.DataAccess;
using Data.Model;
using Data.Utils;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Core
{
    public interface IHangfireServices
    {
        void ScheduleCheckFailureWalletTransaction(Guid walletTransactionId);
        void RecuringUpdateCustomerPriorityEveryMonday();
        void DeleteJobClient(string jobId);
    }

    public class HangfireServices : IHangfireServices
    {
        private readonly IRecurringJobManager _recurringJob;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<HangfireServices> _logger;

        public HangfireServices(
            IRecurringJobManager recurringJobManager,
            IBackgroundJobClient backgroundJobClient,
            AppDbContext dbContext,
            ILogger<HangfireServices> logger
        )
        {
            _recurringJob = recurringJobManager;
            _backgroundJobClient = backgroundJobClient;
            _dbContext = dbContext;
            _logger = logger;
        }

        public void DeleteJobClient(string jobId)
        {
            try
            {
                _backgroundJobClient.Delete(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job with Id: {JobId}", jobId);
            }
        }

        public void ScheduleCheckFailureWalletTransaction(Guid walletTransactionId)
        {
            try
            {
                _backgroundJobClient.Schedule<IWalletService>(methodCall: (_) => _.CheckFailureWalletTransaction(walletTransactionId), delay: TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling CheckFailureWalletTransaction for WalletTransactionId: {WalletTransactionId}", walletTransactionId);
            }
        }

        public void RecuringUpdateCustomerPriorityEveryMonday()
        {
            try
            {
                _recurringJob.AddOrUpdate<IUserService>("UpdateCustomerPriority", methodCall: (_) => _.UpdateCustomerPriority(), cronExpression: Cron.Minutely);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling RecuringUpdateCustomerPriorityEveryMonday");
            }
        }

    }
}
