namespace Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ExternalInterfaces;
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
                Console.ReadKey();
                await using var client = await ConnectClient();

                var listOfTasks = new List<Task>();
                var concurrentDic = new ConcurrentDictionary<int, double>();

                for (var i = 0; i < 1000; i++)
                {
                    var j = i;
                    var cli = client;
                    listOfTasks.Add(Task.Factory.StartNew(async () =>
                    {
                        var grain = cli.GetGrain<IExternalGrain>(j);
                        var response = await grain.Random();
                        concurrentDic.TryAdd(j, response);
                    }));
                }

                Task.WaitAll(listOfTasks.ToArray());

                for (var i = 0; i < 1000; i++)
                    Console.WriteLine($"Grain {i} returned {concurrentDic[i]}");
                        
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
