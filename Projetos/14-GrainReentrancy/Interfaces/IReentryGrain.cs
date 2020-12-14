namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface INormalGrain : IGrainWithIntegerKey
    {
        Task Do();

        Task Make();
    }
}
