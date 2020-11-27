namespace Client
{
    using System;
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

                // grain with guid primary key
                var guidGrainKey = Guid.NewGuid();
                var guidGrain = client.GetGrain<IGuidGrain>(guidGrainKey);
                var guidGrainResult = await guidGrain.GetKey();
                Console.WriteLine($"Grain {nameof(guidGrain)} with key {guidGrainKey} was activated with {guidGrainResult}");

                // grain with long primary key
                var longGrainKey = 1;
                var longGrain = client.GetGrain<ILongGrain>(longGrainKey);
                var longGrainResult = await longGrain.GetKey();
                Console.WriteLine($"Grain {nameof(longGrain)} with key {longGrainKey} was activated with {longGrainResult}");

                // grain with string primary key
                var stringGrainKey = "example";
                var stringGrain = client.GetGrain<IStringGrain>(stringGrainKey);
                var stringGrainResult = await stringGrain.GetKey();
                Console.WriteLine($"Grain {nameof(stringGrain)} with key {stringGrainKey} was activated with {stringGrainResult}");

                // grain with guid primary key and string secondary key
                var compoundGuidString_GuidGrainKey = Guid.NewGuid();
                var compoundGuidString_StringGrainKey = "example2";
                var compoundGuidStringGrain = client.GetGrain<IGuidAndStringGrain>(compoundGuidString_GuidGrainKey, compoundGuidString_StringGrainKey);
                var compoundGuidStringGrainPrimaryResult = await compoundGuidStringGrain.GetKey();
                var compoundGuidStringGrainSecondaryResult = await compoundGuidStringGrain.GetSecondaryKey();
                Console.WriteLine($"Grain {nameof(compoundGuidStringGrain)} with key {compoundGuidString_GuidGrainKey} and extension {compoundGuidString_StringGrainKey} was activated with {compoundGuidStringGrainPrimaryResult} and {compoundGuidStringGrainSecondaryResult}");

                // grain with int primary key and string secondary key
                var compoundIntString_IntGrainKey = 3;
                var compoundIntString_StringGrainKey = "example3";
                var compoundLongStringGrain = client.GetGrain<ILongAndStringGrain>(compoundIntString_IntGrainKey, compoundIntString_StringGrainKey);
                Console.WriteLine($"Grain {nameof(compoundLongStringGrain)} with key {compoundIntString_IntGrainKey} and extension {compoundIntString_StringGrainKey} was activated with {compoundLongStringGrain.GetKey().Result} and {compoundLongStringGrain.GetSecondaryKey().Result}");

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
