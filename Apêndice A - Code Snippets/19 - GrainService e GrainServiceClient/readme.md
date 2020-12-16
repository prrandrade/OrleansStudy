# GrainService e GrainServiceClient

```csharp
// interface de um GrainService implementa IGrainService
public interface IExampleGrainService : IGrainService
{
	Task SomeMethod();
}
```


```csharp
// implementação de um GrainService herda de GrainService e precisa de um
// construtor específico.

// pode receber IGrainFactory par interagir com outros Grains

// atributo reentrant para melhor desempenho se for chamado

// precisa do pacote Microsoft.Orleans.OrleansRuntime instalado
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

	// método init é chamado quando o silo é iniciado
	public override Task Init(IServiceProvider serviceProvider)
	{		
		return base.Init(serviceProvider);
	}

	public Task SomeMethod()
	{
		return Task.CompletedTask;
	}
}
```

```csharp
/// GrainServiceClient é necessário se o Grain precisa lidar com o GrainService
/// interface precisa herdar de IGrainServiceClient<T> e da interface do GrainService correspondente
public interface IExampleGrainServiceClient : IGrainServiceClient<IExampleGrainService>, IExampleGrainService {

}
```

```csharp
// implementação do GrainServiceClient só serve para fazer a ponte com o GrainService correspondente
// herda de GrainServiceClient<T>
public class ExampleGrainServiceClient : GrainServiceClient<IExampleGrainService>, IExampleGrainServiceClient
{
	public ExampleGrainServiceClient(IServiceProvider serviceProvider) : base(serviceProvider)
	{
	}

	// ponte do Grain para o GrainService
	public Task SomeMethod() => GrainService.SomeMethod(i);
}
```

```csharp
// um grain normal recebe o GrainServiceClient via injeção de dependência para usá-lo
public class ExampleGrain : Grain, IExampleGrain
{
	private readonly IExampleGrainServiceClient _grainServiceClient;

	public ExampleGrain(IExampleGrainServiceClient grainServiceClient)
	{
		_grainServiceClient = grainServiceClient;
	}

	public Task CallGrainService()
	{
		return _grainServiceClient.SomeMethod();
	}
}
```

```csharp
// GrainService e GrainServiceCLient precisam ter as injeções configuradas no Silo
var builder = new SiloHostBuilder()
	
	// ... outras configurações...

	.AddGrainService<ExampleGrainService>()
	.ConfigureServices(s =>
	{
		s.AddSingleton<IExampleGrainServiceClient, ExampleGrainServiceClient>();
	});
```

