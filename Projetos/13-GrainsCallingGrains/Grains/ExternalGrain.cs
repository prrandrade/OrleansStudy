namespace Grains
{
    using System.Threading.Tasks;
    using ExternalInterfaces;
    using InternalInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    public class ExternalGrain : Grain, IExternalGrain
    {
        private readonly ILogger<InternalGrain> _logger;
        private readonly IGrainFactory _grainFactory;

        public ExternalGrain(ILogger<InternalGrain> logger, IGrainFactory grainFactory)
        {
            _logger = logger;
            _grainFactory = grainFactory;
        }

        public async Task<double> Random()
        {
            _logger.LogInformation($"Generating external ramdonness for grain {this.GetPrimaryKeyLong()}");
            var internalGrain = _grainFactory.GetGrain<IInternalGrain>(this.GetPrimaryKeyLong());
            return await internalGrain.GenerateSomeRandomness();
        }
    }
}
