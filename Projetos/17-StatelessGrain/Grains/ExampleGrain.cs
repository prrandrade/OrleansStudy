namespace Grains
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans;
    using Orleans.Concurrency;

    [StatelessWorker(1)]
    public class ExampleGrain : Grain, IExampleGrain
    {
        public async Task Do()
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.ffff} - {this.GetPrimaryKeyLong()} - Done!");
            await Task.Delay(500);
        }
    }
}
