namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IExampleGrain : IGrainWithIntegerKey, IRemindable
    {
        Task ActivateReminder();

        Task DeactivateGrain();

        Task DeactivateReminder();
    }
}
