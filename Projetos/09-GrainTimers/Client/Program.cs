namespace Client
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
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
                var grain = client.GetGrain<IExampleGrain>(0);

                // aqui o timer é ativado
                await grain.ActivateTimer();
                Console.ReadKey(true);

                // aqui o timer não será mais ativado, pois já o ativamos
                await grain.ActivateTimer();
                Console.ReadKey(true);

                // aqui o timer será desativado
                await grain.DeactivateTimer();
                Console.ReadKey(true);

                // aqui o timer não será desativado, pois já o desativamos
                await grain.DeactivateTimer();
                Console.ReadKey(true);

                // aqui o timer é ativado novamente
                await grain.ActivateTimer();
                Console.ReadKey(true);

                // aqui o grain é desativado, o que desativa o timer
                await grain.DeactivateGrain();
                Console.ReadKey(true);

                // aqui o timer será ativado (e o grain reativado)
                await grain.ActivateTimer();
                Console.ReadKey(true);

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

                .UseLocalhostClustering()

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
