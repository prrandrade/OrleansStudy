# Projeto GrainsCallingGrains

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Pacotes que precisam ser instalados no Silo](#3-pacotes-que-precisam-ser-instalados-no-silo)
- [Exemplo prático de Grain chamado diretamente no Silo](#4-exemplo-prático-de-grain-chamado-diretamente-no-silo)
- [Client consumindo Grains sem saber](#5-client-consumindo-grains-sem-saber)
- [Sumário](#6-sumário)

# 1. Introdução

Aqui vamos ver um exemplo básico de como podemos chamar Grains dentro de outros Grains, conseguindo espalhar uma única chamada feita pelo **Client** em diferentes **Silos**, já que cada chamada a um **Grain** é distribuída - mesmo quando feita dentro de um **Silo**.

<div align="right">
	
[Voltar](#projeto-grainscallinggrains)

</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker][docker-site]. Use a linha de comando que eu separei no repositório [DockerShortcuts][docker-shortcuts].

<div align="right">
	
[Voltar](#projeto-grainscallinggrains)

</div>

# 3. Como chamar Grains dentro de outros Grains

Podemos ativar e chamar métodos de **Grains** dentro de outros **Grains** (ou seja, em algum **Silo**) de duas formas. Ou chamamos a propriedade **GrainFactory** diretamente dentro do **Grain** ou injetamos a dependência **IGrainFactory** no **Grain** via construtor - meu método preferido para facilitar testes unitários.

A partir do momento que você tem a referência, o funcionamento é literalmente o mesmo de quando precisamos usar **Grains** a partir do **Client** - usando o método `GetGrain<T>` passando a **interface do Grain** e a chave primária - nós nunca usamos diretamente as implementações dos **Grains**, isso é responsabilidade do Orleans.

```csharp
// no client
client.GetGrain<ISomeGrain>(j);

// no grain, chamando outro grain via propriedade GrainFactory
GrainFactory.GetGrain<ISomeGrain>(j);

// no grain, com a dependência IGrainFactory previamente injetada
_grainFactory.GetGrain<ISomeGrain>(j);
```

<div align="right">
	
[Voltar](#projeto-grainscallinggrains)

</div>

# 4. Exemplo prático de Grain chamado diretamente no Silo

Neste exemplo, existem dois **Grains** cujas interfaces estão em dois projetos diferentes. O projeto **ExternalGrains** tem as interfaces que o **Client** e o **Silo** conhecem - neste caso, é a interface `IExternalGrain`. Já o projeto **InternalInterfaces** tem as interfaces que apenas o **Silo** conhece (no caso, `IInternalGrain`), porque o **Client** nem precisa saber que este Grain **existe**.

Sem muito mistério, o `InternalGrain` apenas grava um log no console da aplicação (algum **Silo**) e gera um número aleatório.

```csharp
public class InternalGrain : Grain, IInternalGrain
{
	private readonly ILogger<InternalGrain> _logger;

	public InternalGrain(ILogger<InternalGrain> logger)
	{
		_logger = logger;
	}

	public Task<double> GenerateSomeRandomness()
	{
		_logger.LogInformation($"Generating internal ramdonness for grain {this.GetPrimaryKeyLong()}");
		var r = new Random();
		return Task.FromResult(r.NextDouble());
	}
}
```

Já o `ExternalGrain` recebe `IGrainFactory` via injeção de dependência no construtor, ativa um `InternalGrain` e faz a chamada para devolver o resultado. Como escrito anteriormente, o **Client** que utilizar o `IExternalGrain` nem precisa saber que o `IInternalGrain` existe!

```csharp
public class ExternalGrain : Grain, IExternalGrain
{
	private readonly ILogger<InternalGrain> _logger;
	private readonly IGrainFactory _grainFactory;

	public ExternalGrain(ILogger<InternalGrain> logger, IGrainFactory grainFactory)
	{
		_logger = logger;
		_grainFactory = grainFactory;
	}

	public async Task<double> Random()
	{
		_logger.LogInformation($"Generating external ramdonness for grain {this.GetPrimaryKeyLong()}");
		var internalGrain = _grainFactory.GetGrain<IInternalGrain>(this.GetPrimaryKeyLong());
		return await internalGrain.GenerateSomeRandomness();
	}
}
```

<div align="right">
	
[Voltar](#projeto-grainscallinggrains)

</div>

# 5. Client consumindo Grains sem saber

Neste cenário, podemos subir mais de um **Silo**. O **Client**, ao fazer as chamadas ao `IExternalGrain`, nem vai saber que o `IInternalGrain` também está envolvido. O legal disso tudo é que já sabemos que a execução do código do **Client**, chamando 1000 **Grains**, é feita de forma distribuída entre os **Silos**. Só que as chamadas ao `IInternalGrain`, feitas dentro dos **Silos**, também são distribuídas!

```csharp
Console.ReadKey();
await using var client = await ConnectClient();

var listOfTasks = new List<Task>();
var concurrentDic = new ConcurrentDictionary<int, double>();

for (var i = 0; i < 1000; i++)
{
	var j = i;
	var cli = client;
	listOfTasks.Add(Task.Factory.StartNew(async () =>
	{
		var grain = cli.GetGrain<IExternalGrain>(j);
		var response = await grain.Random();
		concurrentDic.TryAdd(j, response);
	}));
}

Task.WaitAll(listOfTasks.ToArray());
```

Exemplo de log quando dois **Silos** estão sendo executados (repare que a chamada para os **Grains** com a chave primária **876** são feitas entre ambos os **Silos**. Um deles lidou com a chave feita pelo **Client**, o outro lidou com a chamada feita pelo primeiro **Silo**):

```
info: Grains.InternalGrain[0]
      Generating internal ramdonness for grain 876
info: Grains.InternalGrain[0]
      Generating external ramdonness for grain 877
info: Grains.InternalGrain[0]
      Generating external ramdonness for grain 880
info: Grains.InternalGrain[0]
      Generating internal ramdonness for grain 877
info: Grains.InternalGrain[0]
      Generating external ramdonness for grain 882
info: Grains.InternalGrain[0]
      Generating internal ramdonness for grain 880
info: Grains.InternalGrain[0]
      Generating internal ramdonness for grain 882
```

```
info: Grains.InternalGrain[0]
      Generating external ramdonness for grain 876
info: Grains.InternalGrain[0]
      Generating internal ramdonness for grain 874
info: Grains.InternalGrain[0]
      Generating external ramdonness for grain 879
info: Grains.InternalGrain[0]
      Generating internal ramdonness for grain 879
info: Grains.InternalGrain[0]
      Generating internal ramdonness for grain 878
info: Grains.InternalGrain[0]
      Generating external ramdonness for grain 881
info: Grains.InternalGrain[0]
      Generating internal ramdonness for grain 881
```

<div align="right">
	
[Voltar](#projeto-grainscallinggrains)

</div>


# 6. Sumário

- **Grains** podem chamar outros **Grains** normalmente, para distribuir ainda mais o processamento de algo.
- **Clients** nem precisam saber que alguns **Grains** são internos; eles só precisam ter acesso aos **Grains** 'externos'. Isso obviamente varia conforme a organização do seu projeto.

<div align="right">
	
[Voltar](#projeto-grainscallinggrains)

</div>