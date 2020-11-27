namespace Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Orleans;

    internal class Program
    {
        private static async Task<int> Main()
        {
            try
            {
                await using var client = await ConnectClient();

                var t1 = Task.Factory.StartNew(() =>
                {
                    var grain = client.GetGrain<IExampleGrain>(0);
                    Console.WriteLine($"Grain will run Process at {DateTime.Now:HH:mm:ss.fff}");
                    grain.Process().Wait();
                    Console.WriteLine($"Grain returned Process at {DateTime.Now:HH:mm:ss.fff}");
                });
                Thread.Sleep(1000);

                var t2 = Task.Factory.StartNew(() =>
                {
                    var grain = client.GetGrain<IExampleGrain>(0);
                    Console.WriteLine($"Grain will run AnotherProcess at {DateTime.Now:HH:mm:ss.fff}");
                    grain.AnotherProcess().Wait();
                    Console.WriteLine($"Grain return AnotherProcess at {DateTime.Now:HH:mm:ss.fff}");
                });
                
                Task.WaitAll(t1, t2);
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
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host \n");
            return client;
        }
    }
}
