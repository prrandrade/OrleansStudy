namespace Grains
{
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    public class ExampleGrain : Grain, IExampleGrain
    {
        private readonly ILogger<ExampleGrain> _logger;
        private readonly IExampleGrainServiceClient _grainServiceClient;

        public ExampleGrain(ILogger<ExampleGrain> logger, IExampleGrainServiceClient grainServiceClient)
        {
            _logger = logger;
            _grainServiceClient = grainServiceClient;
        }

        public Task CallGrainService()
        {
            _logger.LogInformation($"Method {nameof(CallGrainService)} of grain {nameof(ExampleGrain)}-{this.GetPrimaryKeyLong()} called!");
            Thread.Sleep(2000);
            return _grainServiceClient.CallGrain(this.GetPrimaryKeyLong());
        }
    }
}
