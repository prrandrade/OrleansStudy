namespace Client
{
    using System;
    using System.Collections.Generic;
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
                var grain = client.GetGrain<INormalGrain>(0);
                var reentryGrain = client.GetGrain<IReentryGrain>(0);

                // grain normal
                var listOfTasks = new List<Task>();
                for (var i = 0; i < 10; i++)
                {
                    listOfTasks.Add(Task.Factory.StartNew(async () =>
                    {
                        await grain.Do();
                    }));
                }
                for (var i = 0; i < 10; i++)
                {
                    listOfTasks.Add(Task.Factory.StartNew(async () =>
                    {
                        await grain.Make();
                    }));
                }
                Task.WaitAll(listOfTasks.ToArray());
                listOfTasks.Clear();
                Console.ReadKey();

                // grain com atributo reentry
                for (var i = 0; i < 10; i++)
                {
                    listOfTasks.Add(Task.Factory.StartNew(async () =>
                    {
                        await reentryGrain.Do();
                    }));
                }
                for (var i = 0; i < 10; i++)
                {
                    listOfTasks.Add(Task.Factory.StartNew(async () =>
                    {
                        await reentryGrain.Make();
                    }));
                }
                Task.WaitAll(listOfTasks.ToArray());
                listOfTasks.Clear();
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
