namespace Grains
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans;

    public class GuidGrain : Grain, IGuidGrain
    {
        public Task<Guid> GetKey()
        {
            return Task.FromResult(this.GetPrimaryKey());
        }
    }
}
