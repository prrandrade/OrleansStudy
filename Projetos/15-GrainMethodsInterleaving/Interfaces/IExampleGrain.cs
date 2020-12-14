namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;
    using Orleans.Concurrency;

    public interface IExampleGrain : IGrainWithIntegerKey
    {
        Task DoSlow();

        [AlwaysInterleave]
        Task DoFast();
    }
}
