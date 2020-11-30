namespace Grains
{
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    public class HelloGrain : Grain, IHelloGrain
    {
        private readonly ILogger<HelloGrain> _logger;

        public HelloGrain(ILogger<HelloGrain> logger)
        {
            _logger = logger;
        }

        public Task Ping()
        {
            _logger.LogInformation($"Pinged {this.GetPrimaryKeyLong()}!");
            return Task.CompletedTask;
        }
    }
}
