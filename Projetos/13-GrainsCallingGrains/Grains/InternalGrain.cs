namespace Grains
{
    using System;
    using System.Threading.Tasks;
    using InternalInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    public class InternalGrain : Grain, IInternalGrain
    {
        private readonly ILogger<InternalGrain> _logger;

        public InternalGrain(ILogger<InternalGrain> logger)
        {
            _logger = logger;
        }

        public Task<double> GenerateSomeRandomness()
        {
            _logger.LogInformation($"Generating internal ramdonness for grain {this.GetPrimaryKeyLong()}");
            var r = new Random();
            return Task.FromResult(r.NextDouble());
        }
    }
}
