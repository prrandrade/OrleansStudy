namespace Grains
{
    using System.Threading.Tasks;
    using Interfaces.Grains;
    using Interfaces.Repositories;
    using Models;
    using Orleans;

    public class UserGrain : Grain, IUserGrain
    {
        private readonly IUserRepository _repository;

        public UserGrain(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> RegisterUser(string name)
        {
            if (await _repository.CheckIfExistsAsync(this.GetPrimaryKeyString()))
                return false;

            await _repository.AddAsync(this.GetPrimaryKeyString(), name);
            return true;
        }

        public async Task<UserModel> RetrieveUser()
        {
            return await _repository.RetrieveAsync(this.GetPrimaryKeyString());
        }
    }
}
