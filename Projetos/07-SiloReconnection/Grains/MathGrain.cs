namespace Grains
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    public class MathGrain : Grain, IMathGrain
    {
        private readonly ILogger<MathGrain> _logger;

        public MathGrain(ILogger<MathGrain> logger)
        {
            _logger = logger;
        }

        public override Task OnActivateAsync()
        {
            _logger.LogWarning($"Grain {this.GetPrimaryKeyLong()} is activated!");
            return base.OnActivateAsync();
        }

        public Task CreateLog()
        {
            Thread.Sleep(5000);
            _logger.LogInformation($"Log created for {this.GetPrimaryKeyLong()}!");
            return Task.CompletedTask;
        }

        public Task<int> Pow(uint a, uint b)
        {
            if (b == 0)
                return Task.FromResult(1);

            var r = Convert.ToInt32(a);

            for (var i = 0; i < b; i++)
            {
                Thread.Sleep(500);
                r *= Convert.ToInt32(a);
            }

            return Task.FromResult(r);
        }
    }
}
