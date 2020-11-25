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

        public override Task OnActivateAsync()
        {
            _logger.LogInformation($"Grain with primary key {this.GetPrimaryKeyLong()} is activated.");
            return base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            _logger.LogInformation($"Grain with primary key {this.GetPrimaryKeyLong()} is deactivated.");
            return base.OnDeactivateAsync();
        }

        public Task ExampleMethod(bool withDeactivation)
        {
            if (withDeactivation)
                DeactivateOnIdle();
            return Task.CompletedTask;
        }
    }
}
