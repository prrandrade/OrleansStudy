namespace Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Orleans;

    public interface IConversationGrain : IGrainWithIntegerKey
    {
        Task Say(string message);

        Task<IEnumerable<string>> ShowHistory();

        Task EraseHistory();
    }
}
