namespace Grains
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Concurrency;
    using Orleans.Core;
    using Orleans.Runtime;

    [Reentrant]
    public class ExampleGrainService : GrainService, IExampleGrainService
    {
        private readonly IGrainFactory _grainFactory;
        private readonly ILogger<ExampleGrainService> _logger;

        public ExampleGrainService(IGrainIdentity grainId, Silo silo, 
            ILoggerFactory loggerFactory, ILogger<ExampleGrainService> logger, IGrainFactory grainFactory) : base(grainId, silo, loggerFactory)
        {
            _grainFactory = grainFactory;
            _logger = logger;
        }

        public override Task Init(IServiceProvider serviceProvider)
        {
            Task.Run(() =>
            {
                _logger.LogInformation("Everything will start in 5 seconds...");
                Thread.Sleep(5000);
                _grainFactory.GetGrain<IExampleGrain>(0).CallGrainService();
            });

            return base.Init(serviceProvider);
        }

        public Task CallGrain(long i)
        {
            _logger.LogInformation($"Method {nameof(CallGrain)} of grain {nameof(ExampleGrainService)} called!");

            Task.Run(() =>
            {
                Thread.Sleep(3000);
                _grainFactory.GetGrain<IExampleGrain>(i).CallGrainService();
            });

            return Task.CompletedTask;
        }
    }
}
