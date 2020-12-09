namespace Repositories
{
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper;
    using Interfaces.Repositories;
    using Models;

    public class UserRepository : IUserRepository
    {
        public async Task AddAsync(string identification, string name)
        {
            await using var connection = new SqlConnection("Server=localhost;Database=Business;User Id=sa;Password=root@1234");
            await connection.ExecuteAsync("insert into users (identification, name) values (@identification, @name)", new { identification, name });
            await connection.CloseAsync();
        }

        public async Task<UserModel> RetrieveAsync(string identification)
        {
            await using var connection = new SqlConnection("Server=localhost;Database=Business;User Id=sa;Password=root@1234");
            var result = await connection.QueryAsync<UserModel>("select * from users where identification = @identification", new { identification });
            await connection.CloseAsync();
            return result.FirstOrDefault();
        }

        public async Task<bool> CheckIfExistsAsync(string identification)
        {
            await using var connection = new SqlConnection("Server=localhost;Database=Business;User Id=sa;Password=root@1234");
            return await connection.ExecuteScalarAsync<bool>("select count(*) from users where identification = @identification", new { identification });
        }
    }
}
