namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface ILongGrain : IGrainWithIntegerKey
    {
        Task<long> GetKey();
    }
}
