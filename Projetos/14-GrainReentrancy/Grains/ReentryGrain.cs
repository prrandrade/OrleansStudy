namespace Grains
{
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Concurrency;

    [Reentrant]
    public class ReentryGrain : Grain, IReentryGrain
    {
        private readonly ILogger<IReentryGrain> _logger;

        public ReentryGrain(ILogger<IReentryGrain> logger)
        {
            _logger = logger;
        }

        public async Task Do()
        {
            _logger.LogInformation("WILL DO IT!");
            await Task.Delay(20);
            _logger.LogInformation("DID IT!");
            
        }

        public async Task Make()
        {
            _logger.LogInformation("WILL MAKE IT!");
            await Task.Delay(10);
            _logger.LogInformation("MADE IT!");
        }
    }
}
