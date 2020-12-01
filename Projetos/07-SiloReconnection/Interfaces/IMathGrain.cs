namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IMathGrain : IGrainWithIntegerKey
    {
        Task<int> Pow(uint a, uint b);

        Task CreateLog();
    }
}
