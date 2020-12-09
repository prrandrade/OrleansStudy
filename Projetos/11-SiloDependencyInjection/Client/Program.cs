namespace Client
{
    using System;
    using System.Threading.Tasks;
    using Interfaces.Grains;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Configuration;
    using Orleans.Hosting;

    internal class Program
    {
        private static async Task<int> Main()
        {
            try
            {
                await using var client = await ConnectClient();

                // usuário com identificação 12345
                var user1 = client.GetGrain<IUserGrain>("12345");
                var result1 = await user1.RegisterUser("Fulano");
                if (result1)
                {
                    var user1Result = await user1.RetrieveUser();
                    Console.WriteLine($"{user1Result.Id} - {user1Result.Identification} - {user1Result.Name}");
                }
                Console.ReadKey(true);

                // usuário com identificação 54321 
                var user2 = client.GetGrain<IUserGrain>("54321");
                var result2 = await user2.RegisterUser("Beltrano");
                if (result2)
                {
                    var user2Result = await user2.RetrieveUser();
                    Console.WriteLine($"{user2Result.Id} - {user2Result.Identification} - {user2Result.Name}");
                }
                Console.ReadKey(true);

                // usuário com identificação 11111
                var user3 = client.GetGrain<IUserGrain>("11111");
                var user3Result = await user3.RetrieveUser();
                if (user3Result == null)
                    Console.WriteLine("Nenhum usuário com identificação 11111");
                Console.ReadKey(true);

                // usuário com identificação 12345 novamente
                var user4 = client.GetGrain<IUserGrain>("12345");
                var result4 = await user4.RegisterUser("Outro nome");
                if (!result4)
                    Console.WriteLine("Usuário 12345 já foi registrado!");

                Console.ReadKey();
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nException while trying to run client: {e.Message}");
                Console.WriteLine("Make sure the silo the client is trying to connect to is running.");
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return 1;
            }
        }

        private static async Task<IClusterClient> ConnectClient()
        {
            var client = new ClientBuilder()

                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "dev";
                })

                .UseAdoNetClustering(options =>
                {
                    options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
                    options.Invariant = "System.Data.SqlClient";
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host \n");
            return client;
        }
    }
}
