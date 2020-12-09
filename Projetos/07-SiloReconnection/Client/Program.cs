namespace Client
{
    using System;
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
                var math = client.GetGrain<IMathGrain>(0);
                
                // método de extensão withTimeout
                int resultPow;
                try
                {
                    resultPow = await math
                        .Pow(5, 5)
                        .WithTimeout(TimeSpan.FromMilliseconds(2000), "Timeout for method Generate!");
                }
                catch (TimeoutException t)
                {
                    Console.WriteLine(t);
                    Console.WriteLine("Let's try again!");
                    resultPow = await math
                        .Pow(5, 5)
                        .WithTimeout(TimeSpan.FromDays(1));
                }
                Console.WriteLine($"5^5 = {resultPow}");

                // método de extensão withTimeout
                try
                {
                    await math
                        .CreateLog()
                        .WithTimeout(TimeSpan.FromMilliseconds(2000), "Timeout for method CreateLog!");
                }
                catch (TimeoutException t)
                {
                    Console.WriteLine(t);
                    Console.WriteLine("Let's try again!");
                    await math.CreateLog().WithTimeout(TimeSpan.FromDays(1));
                }
                Console.WriteLine("Log was created and returned!");

                // método de extensão WaitWithThrow
                try
                {
                    math.CreateLog().WaitWithThrow(TimeSpan.FromMilliseconds(2000));
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Let's try again!");
                    math.CreateLog().WaitWithThrow(TimeSpan.FromDays(1));
                }

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
