namespace Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Orleans;

    public interface IGuidAndStringGrain : IGrainWithGuidCompoundKey
    {
        Task<Guid> GetKey();

        Task<string> GetSecondaryKey();
    }
}
