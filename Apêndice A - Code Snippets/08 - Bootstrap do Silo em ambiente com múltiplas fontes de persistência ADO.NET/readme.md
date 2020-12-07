```csharp
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
		var builder = new SiloHostBuilder()

			// configurando cluster
			.Configure<ClusterOptions>(options =>
			{
				options.ClusterId = "dev";
				options.ServiceId = "dev";
			})
			
			// configurando portas usadas pelo Silo, únicas por silo por máquina
			.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)

			// clustering via banco de dados
			.UseAdoNetClustering(options =>
			{
				options.Invariant = "System.Data.SqlClient"; // System.Data.SqlClient ou MySql.Data.MySqlClient ou Npgsql ou Oracle.DataAccess.Client
				options.ConnectionString = "string de conexão";
			})
			
			// persistência via banco de dados com nome
			.AddAdoNetGrainStorage("storage1", options =>
			{
				options.Invariant = "System.Data.SqlClient"; // System.Data.SqlClient ou MySql.Data.MySqlClient ou Npgsql ou Oracle.DataAccess.Client
				options.ConnectionString = "string de conexão";
			})
			
			// outra persistência via banco de dados, pode ser em outra base
			.AddAdoNetGrainStorage("storage2", options =>
			{
				options.Invariant = "System.Data.SqlClient"; // System.Data.SqlClient ou MySql.Data.MySqlClient ou Npgsql ou Oracle.DataAccess.Client
				options.ConnectionString = "string de conexão";
			})

			// reminders via banco de dados
			.UseAdoNetReminderService(options =>
			{
				options.Invariant = "System.Data.SqlClient"; // System.Data.SqlClient ou MySql.Data.MySqlClient ou Npgsql ou Oracle.DataAccess.Client
				options.ConnectionString = "string de conexão";
			})			
			
			.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(**SOME GRAIN**).Assembly).WithReferences());

		var host = builder.Build();
		await host.StartAsync();
		return host;
	}
}
```