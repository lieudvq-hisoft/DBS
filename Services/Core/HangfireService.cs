using Data.DataAccess;
using Hangfire;

namespace Services.Core
{
    public interface IHangfireServices
    {
        void DeleteJobClient(string jobId);
    }
    public class HangfireServices : IHangfireServices
    {
        private readonly IRecurringJobManager _recurringJob;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly AppDbContext _dbContext;
        public HangfireServices(
            IRecurringJobManager recurringJobManager,
            IBackgroundJobClient backgroundJobClient,
            AppDbContext dbContext
            )
        {
            _recurringJob = recurringJobManager;
            _backgroundJobClient = backgroundJobClient;
            _dbContext = dbContext;
        }

        public void DeleteJobClient(string jobId)
        {
            _backgroundJobClient.Delete(jobId);
        }
    }
}
