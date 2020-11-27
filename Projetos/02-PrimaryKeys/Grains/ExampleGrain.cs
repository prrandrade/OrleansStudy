namespace Grains
{
    using System;
    using System.Threading;
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

        public Task Process()
        {
            _logger.LogInformation($"{nameof(Process)} started at {DateTime.Now}");
            Thread.Sleep(3000);
            return Task.CompletedTask;
        }

        public Task AnotherProcess()
        {
            _logger.LogInformation($"{nameof(AnotherProcess)} started at {DateTime.Now}");
            Thread.Sleep(5000);
            return Task.CompletedTask;
        }
    }
}
