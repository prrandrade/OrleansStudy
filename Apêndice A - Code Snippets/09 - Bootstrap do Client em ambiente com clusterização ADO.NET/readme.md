# Bootstrap do Client em ambiente com clusterização ADO.NET

```csharp
internal class Program
{
	private static async Task<int> Main()
	{
		try
		{
			await using var client = await ConnectClient();
			var friend = client.GetGrain<IHelloGrain>(0);
			var response = await friend.SayHello("Good morning, HelloGrain!");
			Console.WriteLine($"\n\n{response}\n\n");
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

			// configurando cluster
			.Configure<ClusterOptions>(options =>
			{
				options.ClusterId = "dev";
				options.ServiceId = "dev";
			})

    			// clustering via banco de dados
			.UseAdoNetClustering(options =>
			{				
				options.Invariant = "System.Data.SqlClient"; // System.Data.SqlClient ou MySql.Data.MySqlClient ou Npgsql ou Oracle.DataAccess.Client
				options.ConnectionString = "string de conexão";
			})
			.Build();

		await client.Connect();
		Console.WriteLine("Client successfully connected to silo host \n");
		return client;
	}
}
```

