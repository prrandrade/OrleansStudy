```csharp
internal class Program
{
	private static async Task<int> Main()
	{
		try
		{
			await using var client = await ConnectClient();
			// uso do client
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
			.Build();

		await client.Connect();
		Console.WriteLine("Client successfully connected to silo host \n");
		return client;
	}
}
```