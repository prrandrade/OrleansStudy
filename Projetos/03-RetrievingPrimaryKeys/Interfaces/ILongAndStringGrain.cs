namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface ILongAndStringGrain : IGrainWithIntegerCompoundKey
    {
        Task<long> GetKey();

        Task<string> GetSecondaryKey();
    }
}
