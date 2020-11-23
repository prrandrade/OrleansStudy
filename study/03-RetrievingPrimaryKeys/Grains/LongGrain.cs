namespace Grains
{
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans;

    public class LongGrain : Grain, ILongGrain
    {
        public Task<long> GetKey()
        {
            return Task.FromResult(this.GetPrimaryKeyLong());
        }
    }
}
