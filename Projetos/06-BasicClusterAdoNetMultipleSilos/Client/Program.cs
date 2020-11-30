namespace Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Configuration;
    using Orleans.Hosting;
    using Orleans.Internal;

    internal class Program
    {
        private static async Task<int> Main()
        {
            try
            {
                await using var client = await ConnectClient();

                var listOfTasks = new List<Task>();
                var listOfResults = new ConcurrentDictionary<int, bool>();

                for (var i = 0; i < 10000; i++)
                {
                    var j = i;
                    listOfTasks.Add(Task.Factory.StartNew(() =>
                    {
                        var friend = client.GetGrain<IHelloGrain>(j);
                        try
                        {
                            friend.Ping().WaitWithThrow(TimeSpan.FromMilliseconds(5000));
                            var t = friend.Ping2().WaitWithThrow(TimeSpan.FromMilliseconds(2000));

                            listOfResults.TryAdd(j, true);
                            Console.WriteLine($"{j} - Pong!");
                        }
                        catch (Exception ex)
                        {
                            listOfResults.TryAdd(j, false);
                            Console.WriteLine($"{j} - Not Pong!");
                        }
                    }));
                }

                Task.WaitAll(listOfTasks.ToArray());
                var numberOfpongs = listOfResults.Values.Count(x => x);
                Console.WriteLine($"{numberOfpongs} calls where ponged!");
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
