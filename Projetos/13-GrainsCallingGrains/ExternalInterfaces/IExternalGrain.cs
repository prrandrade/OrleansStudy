namespace ExternalInterfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IExternalGrain : IGrainWithIntegerKey
    {
        Task<double> Random();
    }
}
