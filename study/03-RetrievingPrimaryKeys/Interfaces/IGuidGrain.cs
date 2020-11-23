namespace Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Orleans;

    public interface IGuidGrain : IGrainWithGuidKey
    {
        Task<Guid> GetKey();
    }
}
