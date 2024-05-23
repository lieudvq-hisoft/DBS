using Microsoft.Extensions.Hosting;
using Services.Core;

namespace Services.Hangfire
{
    public class HangfireJob : BackgroundService
    {
        private readonly IHangfireServices _hangfireServices;

        public HangfireJob(IHangfireServices hangfireServices)
        {
            _hangfireServices = hangfireServices;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() => Start(stoppingToken));
            return Task.CompletedTask;
        }

        private void Start(CancellationToken stoppingToken)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            _hangfireServices.RecuringUpdateCustomerPriorityEveryMonday();
        }
    }

}

