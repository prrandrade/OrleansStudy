# Projeto Hello World

- [Introdução](#1-introdução)
- [Dependências de cada projeto](#2-dependencias-de-cada-projeto)
- [Explicação do projeto de Interfaces](#3-explicação-do-projeto-de-interfaces)
- [Explicação do projeto de Grains](#4-explicação-do-projeto-de-grains)
- [Explicação do projeto do Silo](#5-explicação-do-projeto-do-silo)
- [Explicação do projeto do Client](#6-explicação-do-projeto-do-client)
- [Resultado](#7-resultado)
- [Sumário](#8-sumário)

# 1. Introdução

Vamos entender como funciona a estrutura de um projeto bastante simples do Microsoft Orleans, dividindo-o em quatro partes.

- **Interfaces**: Onde as interfaces dos **Grains** são declaradas - Class Library.
- **Grains**: Onde ficarão as implementações das interfaces, toda a lógica de negócio fica aqui - Class Library.
- **Silo**: É o projeto 'servidor', onde a lógica dos **Grains** é executada - Console Application. 
- **Client**: É o projeto que chama os métodos implementados nos **Grains** - Console Application.

**LEMBRE-SE SEMPRE DE QUE A LÓGICA DE NEGÓCIO DOS GRAINS É CHAMADA A PARTIR DE UM CLIENTE E EXECUTADA NO SILO**

# 2. Dependências de cada projeto

Para que uma estrutura de projetos que usa o Orleans funcione, algumas dependências devem ser previamente instaladas via Nuget:

- Projeto de **Interfaces**
-- Microsoft.Orleans.Core.Abstractions
-- Microsoft.Orleans.CodeGenerator.MSBuild

- Projeto de **Grains**
-- Microsoft.Orleans.Core.Abstractions
-- Microsoft.Orleans.CodeGenerator.MSBuild

- Projeto do **Silo**
-- Microsoft.Orleans.Server

- Projeto do **Client**
-- Microsoft.Orleans.Core

Internamente falando, o projeto de **Interfaces** é referenciado por todos os outros projetos, e o projeto de **Grains** é referenciado pelo projeto do **Silo**, afinal de contas, os serviços que rodam no servidor precisam conhecer a lógica de negócio. Obviamente, outras dependências podem e devem ser instaladas conforme a necessidade e as regras de negócio. O que foi listado aqui é o mínimo necessário para uma solução que usa o Microsoft Orleans.

# 3. Explicação do projeto de Interfaces

Quando um **Grain** é **ativado** - 'construído' no cliente - ele precisa ser ativado com alguma identificação única, literalmente uma chave primária. Por isso, todo o **Grain** precisa implementar uma destas cinco interfaces:

- `IGrainWithIntegerKey` => chave primária é um número inteiro
- `IGrainWithGuidKey` => chave primária é um GUID
- `IGrainWithStringKey` => chave primária é um string
- `IGrainWithGuidCompoundKey` => chave primária composta de string + GUID
- `IGrainWithIntegerCompoundKey` => chave primária composta de string + número inteiro

Neste exemplo bem simples, vamos fazer com que a interface `IHelloGrain` tenha como chave primária de ativação um número inteiro. Como dito anteriormente, todos os métodos de uma interface de **Grain** devem devolver uma `Task` ou uma `ValueTask`.

```csharp
public interface IHelloGrain : IGrainWithIntegerKey
{
	Task<string> SayHello(string greeting);
}
```

# 4. Explicação do projeto de Grains

Todas as implementações de **Grains** precisam obrigatoriamente herdar da classe **Grain** e implementar alguma interface feita no projeto de interfaces. Note que estamos injetando uma dependência de log no construtor do **Grain** - isso já indica que a injeção de dependência pode (e deve) ser usada nos **Grains**. Neste caso específico, o projeto de **Grains** também tem o pacote **Microsoft.Extensions.Logging.Abstractions** instalado, para o uso do `ILogger<>`.

Aqui já vale destacar o comportamento do **Grain**. O log dele **será exibido no Silo**, porque **sua execução é no Silo**, não é no **Client**!

```csharp
public class HelloGrain : Grain, IHelloGrain
{
	private readonly ILogger<HelloGrain> _logger;

	public HelloGrain(ILogger<HelloGrain> logger)
	{
		_logger = logger;
	}

	public Task<string> SayHello(string greeting)
	{
		_logger.LogInformation($"\n SayHello message received: greeting = '{greeting}'");
		return Task.FromResult($"\n Client said: '{greeting}', so HelloGrain says: Hello!");
	}
}
```

# 5. Explicação do projeto do Silo

O projeto do **Silo** simplesmente configura e inicia um `ISiloHost`. O método abaixo cria uma `SiloHostBuilder` adicionando algumas configurações extras:

- `UseLocalhostClustering()` (aplica as configurações de cluster local, útil para ambiente de desenvolvimento)
- `ConfigureApplicationParts` (para que o **Silo** conheça as implementações dos **Grains**)
- `ConfigureLogging` (configuração padrão de log no console, exige o pacote `Microsoft.Extensions.Logging.Console`).

```csharp
private static async Task<ISiloHost> StartSilo()
{
	var builder = new SiloHostBuilder()
		.UseLocalhostClustering()
		.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
		.ConfigureLogging(logging => logging.AddConsole());

	var host = builder.Build();
	await host.StartAsync();
	return host;
}
```

O método `Main` simplesmente é usado para dar iniciar o `ISiloHost`.

```csharp
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
```

# 6. Explicação do projeto do Client

O projeto do **Client** tem um funcionamento bem semelhante ao do **Silo**. Primeiramente precisamos criar um `IClusterClient` com a configuração mais simples possível:

- `UseLocalhostClustering()` (aplica as configurações de cluster local, útil para ambiente de desenvolvimento)
- `ConfigureLogging` (configuração padrão de log no console, exige o pacote `Microsoft.Extensions.Logging.Console`).

```csharp
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
```

O método `Main` do **Client** simplesmente abre a conexão com o Cluster (neste caso, com apenas um **Silo**) e usa métodos da interface `IHelloGrain`, aparentemente sem mistério:

```csharp
static async Task<int> Main(string[] args)
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
```

Vale destacar o que está acontecendo aqui. Como a interface `IHelloGrain` implementa a interface ``IGrainWithIntegerKey` `, quando o **Client** precisa usar a lógica do `IHelloGrain`, este é ativado passando um número inteiro como chave primária:

```csharp
var friend = client.GetGrain<IHelloGrain>(0);
```

Como o projeto do **Client** tem acesso às interfaces dos **Grains**, conseguimos ter acesso e chamar os métodos normalmente.

```csharp
var response = await friend.SayHello("Good morning, HelloGrain!");
```

Mais uma vez, quem está executando o método de fato é o **Silo**. A comunicação entre **Client** e **Silo** é feita de forma transparente - note que, na hora de escrever o código, é como se estivéssemos buscando a implementação de uma interface. Mas acredite, estamos fazendo uma comunicação cliente-servidor aqui, com processamento rodando do lado do servidor!

# 7. Resultado

Fizemos duas aplicações console aqui, o **Silo** e o **Client**. Ao executarmos as duas aplicações (primeiro o **Silo** e depois o **Client**), toda a lógica está no **Grain** e os logs mostrarão o seguinte:

- No **Silo**: **SayHello message received: Good morning, HelloGrain!**, é o log que está no **Grain**, executado no Silo.
- No **Client**: **Client said: 'Good morning, HelloGrain!', so HelloGrain says: Hello!** , é o string devolvido pelo método do **Grain**, recebido pelo Client.


# 8. Sumário

- Interfaces de **Grains** são conhecidas por todos os projetos, e devem implementar uma das interfaces que garante uma chave primaria.

- Métodos dos **Grains** declarados nas intercaces devem sempre devolver uma `Task` ou uma `ValueTask`.

- Implementações dos **Grains**  devem, além de implementar as interfaces, herdar da classe **Grain**
- **Silos** conhecem as interfaces e as implementações dos **Grains**, pois os **Grains** são executados nos **Silos**.

- **Clients** só conhecem as interfaces dos **Grains**, estes se conectam nos **Silos** e recebem o retorno dos métodos.

- Precisamos de uma chave primária para ativar um **Grain** no **Client**.
