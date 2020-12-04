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

                // aqui o reminder é ativado
                await grain.ActivateReminder();
                Console.WriteLine("Reminder ativado/atualizado...");
                Console.ReadKey(true);

                // aqui o reminder é atualizado, pois já existe
                await grain.ActivateReminder();
                Console.WriteLine("Reminder ativado/atualizado...");
                Console.ReadKey(true);

                // aqui o reminder será desativado
                await grain.DeactivateReminder();
                Console.WriteLine("Reminder desativado...");
                Console.ReadKey(true);

                // aqui o reminder será desativado, mas nada acontece porque ele já foi desativado
                await grain.DeactivateReminder();
                Console.WriteLine("Reminder desativado...");
                Console.ReadKey(true);

                // aqui o reminder é ativado novamente
                await grain.ActivateReminder();
                Console.WriteLine("Reminder ativado/atualizado...");
                Console.ReadKey();

                // aqui o grain é desativado, mas o reminder continua,
                // o que fará o grain ser ativado
                // do lado do servidor
                await grain.DeactivateGrain();
                Console.WriteLine("Grain desativado, mas o reminder o reativará...");
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
