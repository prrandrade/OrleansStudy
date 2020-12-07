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




# Introdução

Direto ao ponto, aqui vamos adicionar trechos de código numa espécie de cola rápida para as situações repetitivas que o Orleans tem.

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


