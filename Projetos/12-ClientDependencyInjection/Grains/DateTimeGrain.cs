namespace Grains
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    public class DateTimeGrain : Grain, IDateTimeGrain
    {
        private readonly ILogger<DateTimeGrain> _logger;

        public DateTimeGrain(ILogger<DateTimeGrain> logger)
        {
            _logger = logger;
        }

        public Task<DateTime> CurrentDateTime()
        {
            _logger.LogInformation($"Grain called for {this.GetPrimaryKeyLong()}");
            return Task.FromResult(DateTime.Now);
        }
    }
}
