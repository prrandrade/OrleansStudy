namespace Grains
{
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    public class ExampleGrain : Grain, IExampleGrain
    {
        private readonly ILogger<ExampleGrain> _logger;

        public ExampleGrain(ILogger<ExampleGrain> logger)
        {
            _logger = logger;
        }

        public async Task DoSlow()
        {
            _logger.LogInformation("WILL DO IT SLOW!");
            await Task.Delay(200);
            _logger.LogInformation("DID IT SLOW!");
        }

        public async Task DoFast()
        {
            _logger.LogInformation("WILL DO IT FAST!");
            await Task.Delay(200);
            _logger.LogInformation("DID IT FAST!");
        }
    }
}
