# Projeto ClientDependencyInjection

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Injetando o Client como dependência de outro objeto](#3-injetando-o-client-como-dependência-de-outro-objeto)
- [Exemplo da injeção de dependência em ação](#4-exemplo-da-injeção-de-dependência-em-ação)
- [Sumário](#5-sumário)

# 1. Introdução

Ao contrário dos **Silos**, os **Clients** são a dependência de outros objetos. Neste exemplo, vamos ver como uma WebApi pode se conectar a um **Cluster** do Orleans durante as requisições.

<div align="right">
	
[Voltar](#projeto-clientdependencyinjection)

</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker](https://github.com/prrandrade/DockerShortcuts). Use a linha de comando que eu separei no repositório [DockerShortcuts](https://github.com/prrandrade/DockerShortcuts).

<div align="right">
	
[Voltar](#projeto-clientdependencyinjection)

</div>

# 3. Injetando o Client como dependência de outro objeto

Você já deve ter reparado em todos os outros exemplos, mas friamente falando, quando estamos falando de **Client**, estritamente estamos falando de um objeto do tipo `IClusterClient`.

Repare no método abaixo, que é o que usamos para criar o **Client** de fato. Usamos um **ClientBuilder** para configurar o **Client** com as configurações já explicadas anteriormente e depois, através do método `Connect`, fazemos a conexão com o **Cluster**. Este método retorna o `ClusterClient` que queremos injetar como dependência.

```csharp
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

Numa WebApi, por exemplo, a injeção de dependência é feita através do método `ConfigureServices`. Mas não estamos fazendo uma injeção de dependência qualquer, estamos fazendo a injeção de uma instância específica. Quando o primeiro objeto que precisar se conectar ao **Client** for chamado, o método `ConnectClient` é chamado e injeta o `IClusterClient`.

```csharp
public void ConfigureServices(IServiceCollection services)
{
	// outras injeções...
	services.AddSingleton(_ => ConnectClient().Result);
}
```

O uso do `IClusterClient` como uma dependência via construtor é exatamente igual ao de qualquer outra dependência. O exemplo abaixo simplesmente mostra a dependência em ação num `Controller` da WebApi, o qual apresenta uma `Action` que recebe um parâmetro e faz a chamada no `Client`.

```csharp
[ApiController]
[Route("[controller]")]
public class DateTimeController : ControllerBase
{
	private readonly IClusterClient _client;

	public DateTimeController(IClusterClient client)
	{
		_client = client;
	}

	[HttpGet]
	public async Task<IActionResult> Get(int id)
	{
		var grain = _client.GetGrain<IDateTimeGrain>(id);
		return Ok(await grain.CurrentDateTime());
	}
}
```

O **Grain** deste projeto é propositalmente simples, pois o foco aqui é mostrar a injeção de dependência no **Client**.

```csharp
public class DateTimeGrain : Grain, IDateTimeGrain
{
	private readonly ILogger<DateTimeGrain> _logger;

	public DateTimeGrain(ILogger<DateTimeGrain> logger)
	{
		_logger = logger;
	}

	public Task<DateTime> CurrentDateTime()
	{
		_logger.LogInformation($"Grain called for {this.GetPrimaryKeyLong()}");
		return Task.FromResult(DateTime.Now);
	}
}
```

<div align="right">
	
[Voltar](#projeto-clientdependencyinjection)

</div>

# 4. Exemplo da injeção de dependência em ação

Não há nenhum mistério. Ao fazermos a chamada a `Action` correspondente recebemos a resposta de data e hora vinda do **Grain**, processado no **Silo**, claro.

```
chamada para https://localhost:5001/datetime?id=1
retorno: "2020-12-10T14:59:13.7452081-03:00"
```

Enquanto isso, o **Grain** mostra no log a chave primária passada.

```
info: Grains.DateTimeGrain[0]
      Grain called for 1
```

<div align="right">
	
[Voltar](#projeto-clientdependencyinjection)

</div>

# 5. Sumário

- **Clients** podem e devem ser usados em mecanismos de injeção de dependência para que outros sistemas acessem o **Cluster**.
- **Clients** são usados apenas para a conexão com os **Silos**. Aspectos de segurança como autenticação e autorização não são da alçada do Orleans.

<div align="right">
	
[Voltar](#projeto-clientdependencyinjection)

</div>