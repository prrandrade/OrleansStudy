namespace Client
{
    using System;
    using System.Linq;
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

                // first grain
                var conversation = client.GetGrain<IConversationGrain>(0);
                await conversation.Say("First message");
                await conversation.Say("Second message");
                await conversation.Say("Third message");
                var resultConversation = (await conversation.ShowHistory()).ToList();
                Console.WriteLine($"Conversation with Primary Key 0 has {resultConversation.Count} messages");
                foreach (var s in resultConversation)
                    Console.WriteLine(s);

                // second grain
                var otherConversation = client.GetGrain<IConversationGrain>(1);
                await otherConversation.Say("Another message");
                await otherConversation.Say("And another message");
                var resultOtherConversation = (await otherConversation.ShowHistory()).ToList();
                Console.WriteLine($"Conversation with Primary Key 1 has {resultOtherConversation.Count} messages");
                foreach (var s in resultOtherConversation)
                    Console.WriteLine(s);

                // second grain, erasing history
                await otherConversation.EraseHistory();
                resultOtherConversation = (await otherConversation.ShowHistory()).ToList();
                Console.WriteLine($"Conversation with Primary Key 1 has {resultOtherConversation.Count} messages");

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
