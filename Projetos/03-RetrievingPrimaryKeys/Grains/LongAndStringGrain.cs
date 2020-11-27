namespace Grains
{
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans;

    public class LongAndStringGrain : Grain, ILongAndStringGrain
    {
        public Task<long> GetKey()
        {
            return Task.FromResult(this.GetPrimaryKeyLong(out _));
        }

        public Task<string> GetSecondaryKey()
        {
            this.GetPrimaryKeyLong(out var keyExt);
            return Task.FromResult(keyExt);
        }
    }
}
