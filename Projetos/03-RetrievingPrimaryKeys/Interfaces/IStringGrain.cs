namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IStringGrain : IGrainWithStringKey
    {
        Task<string> GetKey();
    }
}
