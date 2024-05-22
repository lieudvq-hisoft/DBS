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
        Task<ResultModel> CheckFailureWalletTransaction(Guid walletTransactionId);
        void ScheduleCheckFailureWalletTransaction(Guid walletTransactionId);
        Task<ResultModel> UpdateCustomerPriority();
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

        public async Task<ResultModel> CheckFailureWalletTransaction(Guid walletTransactionId)
        {
            var result = new ResultModel();

            try
            {
                var walletTransaction = await _dbContext.WalletTransactions
                    .FirstOrDefaultAsync(wt => wt.Id == walletTransactionId);

                if (walletTransaction != null && walletTransaction.Status == Data.Enums.WalletTransactionStatus.Waiting)
                {
                    walletTransaction.Status = Data.Enums.WalletTransactionStatus.Failure;
                    await _dbContext.SaveChangesAsync();
                }
                result.Succeed = true;
                result.Data = walletTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckFailureWalletTransaction for WalletTransactionId: {WalletTransactionId}", walletTransactionId);
            }
            return result;
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
                var id = _backgroundJobClient.Schedule(() => CheckFailureWalletTransaction(walletTransactionId), TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling CheckFailureWalletTransaction for WalletTransactionId: {WalletTransactionId}", walletTransactionId);
            }
        }

        public async Task<ResultModel> UpdateCustomerPriority()
        {
            var result = new ResultModel();
            try
            {
                var customers = await _dbContext.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .Where(u => u.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Customer)
                        && u.Priority < 2
                        && !u.IsDeleted)
                    .ToListAsync();

                foreach (var customer in customers)
                {
                    customer.Priority = 2;
                }
                await _dbContext.SaveChangesAsync();

                result.Succeed = true;
                result.Data = customers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer priority");
            }
            return result;
        }

        public void RecuringUpdateCustomerPriorityEveryMonday()
        {
            try
            {
                _recurringJob.AddOrUpdate<IHangfireServices>("UpdateCustomerPriority", methodCall: (_) => _.UpdateCustomerPriority(), Cron.Weekly(DayOfWeek.Monday, 0, 0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling RecuringUpdateCustomerPriorityEveryMonday");
            }
        }

    }
}
