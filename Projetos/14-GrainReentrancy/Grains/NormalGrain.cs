namespace Grains
{
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    public class NormalGrain : Grain, INormalGrain
    {
        private readonly ILogger<NormalGrain> _logger;

        public NormalGrain(ILogger<NormalGrain> logger)
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
