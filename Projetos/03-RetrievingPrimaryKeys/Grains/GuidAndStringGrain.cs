namespace Grains
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans;

    public class GuidAndStringGrain : Grain, IGuidAndStringGrain
    {
        public Task<Guid> GetKey()
        {
            return Task.FromResult(this.GetPrimaryKey(out _));
        }

        public Task<string> GetSecondaryKey()
        {
            this.GetPrimaryKey(out var keyExt);
            return Task.FromResult(keyExt);
        }
    }
}
