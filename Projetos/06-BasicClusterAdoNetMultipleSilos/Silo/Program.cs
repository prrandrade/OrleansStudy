namespace Silo
{
    using System;
    using System.Threading.Tasks;
    using Grains;
    using Microsoft.Extensions.Logging;
    using Orleans;
    using Orleans.Configuration;
    using Orleans.Hosting;

    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("\n\n Press Enter to terminate...\n\n");
                Console.ReadLine();
                await host.StopAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            int siloPort;
            int gatewayPort;

            try
            {
                siloPort = Convert.ToInt32(Environment.GetCommandLineArgs()[1]);
                gatewayPort = Convert.ToInt32(Environment.GetCommandLineArgs()[2]);
            }
            catch
            {
                siloPort = 11111;
                gatewayPort = 30000;
            }

            var builder = new SiloHostBuilder()

                // configurando acesso ao silo
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "dev";
                })
                .ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort)

                // clustering organizado via banco de dados
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
                })

                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
