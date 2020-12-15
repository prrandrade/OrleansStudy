namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans.Services;

    public interface IExampleGrainService : IGrainService
    {
        Task CallGrain(long i);
    }
}
