# Injeção de dependência no Client

```csharp
// método que cria e conecta o client no cluster
public async Task<IClusterClient> ConnectClient()
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
			options.Invariant = "System.Data.SqlClient";
			options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
		})
		.Build();

	await client.Connect();
	return client;
}
```

```csharp
// criando coleção de serviços (se necessário)
var services = new ServiceCollection();

// adicionando a dependência
services.AddSingleton(_ => ConnectClient().Result);
```