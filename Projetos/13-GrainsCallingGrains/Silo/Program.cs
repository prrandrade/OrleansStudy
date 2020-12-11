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
            var siloPort = Convert.ToInt32(Environment.GetCommandLineArgs()[1]);
            var gatewayPort = Convert.ToInt32(Environment.GetCommandLineArgs()[2]);
            
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

                // tarefas agendadas organizadas via banco de dados
                .UseAdoNetReminderService(options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
                })

                // persistência de objetos organizada via banco de dados
                .AddAdoNetGrainStorageAsDefault(options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
                })
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(InternalGrain).Assembly).WithReferences();
                })
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
