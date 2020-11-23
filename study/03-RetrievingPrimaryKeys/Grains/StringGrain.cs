namespace Grains
{
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans;

    public class StringGrain : Grain, IStringGrain
    {
        public Task<string> GetKey()
        {
            return Task.FromResult(this.GetPrimaryKeyString());
        }
    }
}
