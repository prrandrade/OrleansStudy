# Projeto GrainService

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Criando um GrainService](#3-criando-um-grainservice)
- [Consumindo um GrainService em outros Grains](#4-consumindo-um-grainservice-em-outros-grains)
- [Quem precisa de Client?](#5-quem-precisa-de-client)
- [Sumário](#6-sumário)

# 1. Introdução

Neste projeto, vamos ver com calma como criar um `GrainService`, um tipo especial de **Grain** que existe em todos os **Silos**, já é iniciado e ativado quando o **Silo** é iniciado, sempre está ativo na memória e pode interagir com **Grains** normais.

<div align="right">
	
[Voltar](#projeto-grainservice)

</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker](https://www.docker.com). Use a linha de comando que eu separei no repositório [DockerShortcuts](https://github.com/prrandrade/DockerShortcuts).

<div align="right">
	
[Voltar](#projeto-grainservice)

</div>

# 3. Criando um GrainService

O projeto que terá as implementações do tipo `GrainService` precisa obrigatoriamente do pacote nuget **Microsoft.Orleans.Orleans** - pode um projeto separado, pode ser o mesmo projeto dos **Grains** normais.

No caso das interfaces, não há mistério. Simplesmente interfaces de **GrainServices** implementam a interface `IGrainService`.

```csharp
public interface IExampleGrainService : IGrainService
{
	Task CallGrain(long i);
}
```

A implementação de um **GrainService** precisa herdar da classe `GrainService`, implementar a respectiva interface e obrigatoriamente ter um construtor especial, porque o construtor da classe base precisa receber um `IGrainIdentity`, um `Silo` e um `ILoggerFactory`. Então, no mínimo, o construtor de todo **GrainService** é assim:

```csharp
public class ExampleGrainService : GrainService, IExampleGrainService
{
	public ExampleGrainService(IGrainIdentity grainId, Silo silo, 
		ILoggerFactory loggerFactory) : base(grainId, silo, loggerFactory)
	{

	}
}
```

Só que podemos fazer algumas modificações na classe e no construtor para aumentar o desempenho. Veja, um **GrainService** não deixa de ser um **Grain**, então as mesmas regras de virtual actor valem para ele. Ou seja, uma execução por vez de um método - mas [isso não vale se usamos o atributo `Reentrant`, lembra](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/14-GrainReentrancy)?

E a injeção de dependência continua valendo no **GrainService**, [podemos injetar um `IGrainFactory`](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/11-SiloDependencyInjection) para que o **GrainService**, que obviamente é executado no **Silo**, possa usar outros **Grains**.


```csharp
[Reentrant]
public class ExampleGrainService : GrainService, IExampleGrainService
{
	private readonly IGrainFactory _grainFactory;
	private readonly ILogger<ExampleGrainService> _logger;

	public ExampleGrainService(IGrainIdentity grainId, Silo silo, 
		ILoggerFactory loggerFactory, ILogger<ExampleGrainService> logger, IGrainFactory grainFactory) : base(grainId, silo, loggerFactory)
	{
		_grainFactory = grainFactory;
		_logger = logger;
	}
}
```

A regra de negócio customizada deste projeto é bem simples. O **GrainService** dispara uma tarefa apartada que chama um método de um **Grain** normal, via a injeção de dependência de `IGrainFactory`.

```csharp
public Task CallGrain(long i)
{
	_logger.LogInformation($"Method {nameof(CallGrain)} of grain {nameof(ExampleGrainService)} called!");

	Task.Run(() =>
	{
		Thread.Sleep(3000);
		_grainFactory.GetGrain<IExampleGrain>(i).CallGrainService();
	});

	return Task.CompletedTask;
}
```

Lembra que uma das características do **GrainService** é a de que ele sempre inicia junto com o **Silo**? Então, quando isso acontece o método `Init` é chamado. Vamos fazer uma sobrecarga do método `Init` para que, em cinco segundos, o **Grain** normal seja chamado.

```csharp
public override Task Init(IServiceProvider serviceProvider)
{
	Task.Run(async () =>
	{
		_logger.LogInformation("Everything will start in 5 seconds...");
        Thread.Sleep(5000);                _grainFactory.GetGrain<IExampleGrain>(0).CallGrainService();
	});

	return base.Init(serviceProvider);
}
```

Agora para que o **GrainService** funcione de fato, precisamos registrar a dependência nas configurações do **Silo**, usando o método `AddGrainService`. Note que este método genérico usa a própria implementação do **GrainService** como parâmetro para a declaração de dependência.

```csharp
 var builder = new SiloHostBuilder()
 
	// outras configurações
 
	.AddGrainService<ExampleGrainService>()
	
	// outras configurações
	
```

<div align="right">
	
[Voltar](#projeto-grainservice)

</div>

# 4. Consumindo um GrainService em outros Grains

Veja, a construção feita até aqui funciona muito bem para que o **GrainService** consuma outros **Grains**. Mas o contrário não é válido! Você não pode acessar um **GrainService** através do **Grain**, uma ponte é necessária: o **GrainServiceClient**. Todo **GrainServiceClient** também se divide em interface e implementação. A interface precisa implementar a mesma interface do **GrainService** correspondente e também a interface `IGrainServiceClient<T>`, onde `T` é a mesma interface do **GrainService**:

```csharp
public interface IExampleGrainServiceClient : IGrainServiceClient<IExampleGrainService>, IExampleGrainService { }
```

Já a implementação precisa herdar da classe `GrainServiceClient<T>`, onde `T` é a interface do **GrainService** e a interface do **GrainServiceClient**, que acabamos de criar. Um GrainServiceClient nada mais é que uma ponte entre os **Grains** e um **GrainService**, então o trabalho aqui é apenas e somente fazer com que os métodos sejam redirecionados para os mesmos métodos do **GrainService** (através da propriedade **GrainService**):

```csharp
public class ExampleGrainServiceClient : GrainServiceClient<IExampleGrainService>, IExampleGrainServiceClient
{
	public ExampleGrainServiceClient(IServiceProvider serviceProvider) : base(serviceProvider)
	{
	}

	// ponte do Grain para o GrainService
	public Task CallGrain(long i) => GrainService.CallGrain(i);
}
```

Para que um **Grain** normal use um **GrainService**, basta injetar a interface do **GrainServiceClient** correspondente, que faz a ponte. O legal disso é que esta chamada é distribuída - um **GrainService** que está em outro **Silo** pode atender a chamada.

```csharp
public class ExampleGrain : Grain, IExampleGrain
{
	private readonly ILogger<ExampleGrain> _logger;
	private readonly IExampleGrainServiceClient _grainServiceClient;

	public ExampleGrain(ILogger<ExampleGrain> logger, IExampleGrainServiceClient grainServiceClient)
	{
		_logger = logger;
		_grainServiceClient = grainServiceClient;
	}

	public Task CallGrainService()
	{
		_logger.LogInformation($"Method {nameof(CallGrainService)} of grain {nameof(ExampleGrain)}-{this.GetPrimaryKeyLong()} called!");
		Thread.Sleep(2000);
		return _grainServiceClient.CallGrain(this.GetPrimaryKeyLong());
	}
}
```

Por último, a configuração de injeção de dependência do GrainServiceClient também precisa ser feita nas configurações do **Silo** - [é uma injeção de dependência mais normal no Silo](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/11-SiloDependencyInjection).

```csharp
var builder = new SiloHostBuilder()

	// outras configurações

	.ConfigureServices(s =>
	{
	s.AddSingleton<IExampleGrainServiceClient, ExampleGrainServiceClient>();
	})
	
	// outras configurações
	
```

<div align="right">
	
[Voltar](#projeto-grainservice)

</div>

# 5. Quem precisa de Client?

Agora vamos analisar o que está acontecendo aqui: Sabemos que o **GrainService** é inicializado juntamente com o **Silo**, então sabemos que em primeiro lugar é seu método `Init` que é executado. Aqui, estamos dando um fôlego de cinco segundos e depois chamando o método `CallGrainService` do **Grain** `IExampleGrain`.

```csharp
public override Task Init(IServiceProvider serviceProvider)
{
	Task.Run(() =>
	{
		_logger.LogInformation("Everything will start in 5 seconds...");
		Thread.Sleep(5000);
		_grainFactory.GetGrain<IExampleGrain>(0).CallGrainService();
	});

	return base.Init(serviceProvider);
}
```

Já no **Grain**, nós damos um descanso de 2 segundos e chamados o método `CallGrain` do **GrainServiceClient** - que, sabemos, chama o mesmo método do **GrainService** correspondente.

```csharp
public Task CallGrainService()
{
	_logger.LogInformation($"Method {nameof(CallGrainService)} of grain {nameof(ExampleGrain)}-{this.GetPrimaryKeyLong()} called!");
	Thread.Sleep(2000);
	return _grainServiceClient.CallGrain(this.GetPrimaryKeyLong());
}
```

E de volta ao **GrainService** estamos dando um descanso de 3 segundos e chamando novamente o método `CallGrainService` do **Grain**.

```csharp
public Task CallGrain(long i)
{
	_logger.LogInformation($"Method {nameof(CallGrain)} of grain {nameof(ExampleGrainService)} called!");

	Task.Run(() =>
	{
		Thread.Sleep(3000);
		_grainFactory.GetGrain<IExampleGrain>(i).CallGrainService();
	});

	return Task.CompletedTask;
}
```

De forma resumida, fizemos um ‘bate-rebate’ proposital entre **Grain** e **GrainService**. Isso mostra que podemos criar uma lógica inteira de negócio a partir do **GrainService** sem precisar de nenhuma intervenção do **Client** - Aliás, este projeto nem tem um **Client**! O log do Silo ficará mais ou menos assim:

```csharp
Everything will start in 5 seconds...
...
Method CallGrainService of grain ExampleGrain-0 called!
...
Method CallGrain of grain ExampleGrainService called!
...
Method CallGrainService of grain ExampleGrain-0 called!
...
Method CallGrain of grain ExampleGrainService called!
...
Method CallGrainService of grain ExampleGrain-0 called!
...
Method CallGrain of grain ExampleGrainService called!
...
```
<div align="right">
	
[Voltar](#projeto-grainservice)

</div>

# 6. Sumário

- Um **GrainService** sempre estará ativo quando o **Silo** é iniciado, não é necessário um estímulo externo.
- O **GrainService**, através da injeção de dependência, pode usar outros **Grains** normalmente.
- O **GrainService** aceita outros serviços via injeção de dependência. Portanto, ele pode ser usado para iniciar serviços de escuta de dados para que um sistema distribuído com o Orleans nem preciso de Clients.
- Quando o **Grain** precisa acessar o **GrainService**, um **GrainServiceClient** é necessário para fazer a ponte.

<div align="right">
	
[Voltar](#projeto-grainservice)

</div>