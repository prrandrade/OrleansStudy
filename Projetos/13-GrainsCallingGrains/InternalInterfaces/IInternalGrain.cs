namespace InternalInterfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IInternalGrain : IGrainWithIntegerKey
    {
        Task<double> GenerateSomeRandomness();
    }
}
