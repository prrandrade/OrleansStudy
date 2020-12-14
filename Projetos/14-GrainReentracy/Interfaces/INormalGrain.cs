namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IReentryGrain : IGrainWithIntegerKey
    {
        Task Do();

        Task Make();
    }
}
