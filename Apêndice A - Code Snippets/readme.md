# Apêndice A - Code Snippets

- [Introdução](#introdução)

<!-- Básico -->
- [Pacotes necessários para o projeto de interfaces de Grains](#pacotes-necessários-para-o-projeto-de-interfaces-de-grains)
- [Pacotes necessários para o projeto de implementações de Grains](#pacotes-necessários-para-o-projeto-de-implementações-de-grains)
- [Pacotes necessários para o projeto do Silo](#pacotes-necessários-para-o-projeto-do-silo)
- [Pacotes necessários para o projeto do Client](#pacotes-necessários-para-o-projeto-do-client)
- [Bootstrap do Silo em ambiente local](#bootstrap-do-silo-em-ambiente-local)
- [Bootstrap do Client em ambiente local](#bootstrap-do-client-em-ambiente-local)

<!-- Básico com logging -->
- [Pacotes necessários para o projeto do Silo com logging no console](#pacotes-necessários-para-o-projeto-do-silo-com-logging-no-console)
- [Pacotes necessários para o projeto do Client com logging no console](#pacotes-necessários-para-o-projeto-do-client-com-logging-no-console)
- [Bootstrap do Silo em ambiente local com logging no console](#bootstrap-do-silo-em-ambiente-local-com-logging-no-console)
- [Bootstrap do Client em ambiente local com logging no console](#bootstrap-do-client-em-ambiente-local-com-logging-no-console)

<!-- Básico dos Grains -->
- [Implementando e recuperando chaves primárias dos Grains](#implementando-e-recuperando-chaves-primárias-dos-grains)
- [Sobrecarga na ativação e desativação dos Grains](#sobrecarga-na-ativação-e-desativação-dos-grains)

<!-- Clusterização, persistência e reminders no Silo/CLient -->
- [Pacotes necessários para o projeto do Silo com clusterização ADO.NET](#pacotes-necessários-para-o-projeto-do-silo-com-clusterização-ado.net)
- [Pacotes necessários para o projeto do Silo com persistência ADO.NET](#pacotes-necessários-para-o-projeto-do-silo-com-persistência-ado.net)
- [Pacotes necessários para o projeto do Silo com reminders ADO.NET](#pacotes-necessários-para-o-projeto-do-silo-com-reminders-ado.net)
- [Pacotes necessários para o projeto do Client com clusterização ADO.NET](#pacotes-necessários-para-o-projeto-do-client-com-clusterização-ado.net)

<!-- Códigos para Silo e Client com Clusterização, Persistência e Reminders -->
- [Bootstrap do Silo em ambiente com clusterização, persistência e reminders ADO.NET](#bootstrap-do-silo-em-ambiente-com-clusterização,-persistência-e-reminders-ado.net)
- [Bootstrap do Client em ambiente com clusterização ADO.NET](#bootstrap-do-client-em-ambiente-com-clusterização-ado.net)

<!-- Timers e Reminders nos Grains -->


# Introdução

Direto ao ponto, aqui vamos adicionar trechos de código numa espécie de cola rápida para as situações repetitivas que o Orleans tem - não haverá explicações sobre o funcionamento dos códigos aqui.

# Pacotes necessários para o projeto de interfaces de Grains

- Pacote nuget **Microsoft.Orleans.Core.Abstractions**
- Pacote nuget **Microsoft.Orleans.CodeGenerator.MSBuild**

# Pacotes necessários para o projeto de implementações de Grains

- Pacote nuget **Microsoft.Orleans.Core.Abstractions**
- Pacote nuget **Microsoft.Orleans.CodeGenerator.MSBuild**
- Projeto de **Interfaces dos Grains**

# Pacotes necessários para o projeto do Silo

- Pacote nuget **Microsoft.Orleans.Server**
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Client

- Pacote nuget **Microsoft.Orleans.Core**
- Projeto de **Interfaces dos Grains**

# Bootstrap do Silo em ambiente local

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
			.UseLocalhostClustering()
			.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(**SOME GRAIN**).Assembly).WithReferences());
			.ConfigureLogging(logging => logging.AddConsole());

		var host = builder.Build();
		await host.StartAsync();
		return host;
	}
}
```

# Bootstrap do Client em ambiente local

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

# Pacotes necessários para o projeto do Silo com logging no console

- Pacote nuget **Microsoft.Extensions.Logging.Console**
- Pacote nuget **Microsoft.Orleans.Server**
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Client com logging no console

- Pacote nuget **Microsoft.Extensions.Logging.Console**
- Pacote nuget **Microsoft.Orleans.Core**
- Projeto de **Interfaces dos Grains**


# Bootstrap do Silo em ambiente local com logging no console

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
			.UseLocalhostClustering()
			.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(**SOME GRAIN**).Assembly).WithReferences());
			.ConfigureLogging(logging => logging.AddConsole());

		var host = builder.Build();
		await host.StartAsync();
		return host;
	}
}
```

# Pacotes necessários para o projeto do Client

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
			.ConfigureLogging(logging => logging.AddConsole())
			.Build();

		await client.Connect();
		Console.WriteLine("Client successfully connected to silo host \n");
		return client;
	}
}
```

# Implementando e recuperando chaves primárias dos Grains

```csharp
// chave GUID

public interface IGuidGrain : IGrainWithGuidKey
{
	Task<Guid> GetKey();
}

public class GuidGrain : Grain, IGuidGrain
{
	public Task<Guid> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKey());
	}
}
```

```csharp
// chave LONG

public interface ILongGrain : IGrainWithIntegerKey
{
	Task<long> GetKey();
}

public class LongGrain : Grain, ILongGrain
{
	public Task<long> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKeyLong());
	}
}
```

```csharp
// chave STRING

public interface IStringGrain : IGrainWithStringKey
{
	Task<string> GetKey();
}

public class StringGrain : Grain, IStringGrain
{
	public Task<string> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKeyString());
	}
}
```

```csharp
// chave COMPOSTA GUID + STRING

public interface IGuidAndStringGrain : IGrainWithGuidCompoundKey
{
	Task<Guid> GetKey();

	Task<string> GetSecondaryKey();
}

public class GuidAndStringGrain : Grain, IGuidAndStringGrain
{
	public Task<Guid> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKey(out _));
	}

	public Task<string> GetSecondaryKey()
	{
		this.GetPrimaryKey(out var keyExt);
		return Task.FromResult(keyExt);
	}
}
```

```csharp
// chave COMPOSTA LONG + STRING

public interface ILongAndStringGrain : IGrainWithIntegerCompoundKey
{
	Task<long> GetKey();

	Task<string> GetSecondaryKey();
}

public class LongAndStringGrain : Grain, ILongAndStringGrain
{
	public Task<long> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKeyLong(out _));
	}

	public Task<string> GetSecondaryKey()
	{
		this.GetPrimaryKeyLong(out var keyExt);
		return Task.FromResult(keyExt);
	}
}
```

# Sobrecarga na ativação e desativação dos Grains

```csharp
public interface IExampleGrain : IGrainWithIntegerKey
{
	Task DeactivateGrainNow();
}

public class ExampleGrain : Grain, IExampleGrain
{
	public override Task OnActivateAsync()
	{
		// sobrecarga na ativação do grain
		return base.OnActivateAsync();
	}

	public override Task OnDeactivateAsync()
	{
		// sobrecarga na desativação do grain
		return base.OnDeactivateAsync();
	}
	
	public Task DeactivateGrainNow()
	{
		// este método desativa o Grain assim que possível
		DeactivateOnIdle();
		return Task.CompletedTask;
	}
}
```

# Pacotes necessários para o projeto do Silo com clusterização ADO.NET

- Pacote nuget **Microsoft.Orleans.Server**
- Pacote nuget **Microsoft.Orleans.Clustering.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Silo com persistência ADO.NET

- Pacote nuget **Microsoft.Orleans.Server**
- Pacote nuget **Microsoft.Orleans.Persistence.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Silo com reminders ADO.NET

- Pacote nuget **Microsoft.Orleans.Server**
- Pacote nuget **Microsoft.Orleans.Reminders.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Client com clusterização ADO.NET

- Pacote nuget **Microsoft.Orleans.Core**
- Pacote nuget **Microsoft.Orleans.Clustering.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Bootstrap do Silo em ambiente com clusterização, persistência e reminders ADO.NET

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
			
			// persistência via banco de dados
			.AddAdoNetGrainStorageAsDefault(options =>
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
