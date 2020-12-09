namespace Interfaces.Repositories
{
    using System.Threading.Tasks;
    using Models;

    public interface IUserRepository
    {
        public Task AddAsync(string identification, string name);

        public Task<UserModel> RetrieveAsync(string identification);

        public Task<bool> CheckIfExistsAsync(string identification);
    }
}
