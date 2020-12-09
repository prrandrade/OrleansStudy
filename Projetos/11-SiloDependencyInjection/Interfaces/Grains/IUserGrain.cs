namespace Interfaces.Grains
{
    using System.Threading.Tasks;
    using Models;
    using Orleans;

    public interface IUserGrain : IGrainWithStringKey
    {
        Task<bool> RegisterUser(string name);

        Task<UserModel> RetrieveUser();
    }
}
