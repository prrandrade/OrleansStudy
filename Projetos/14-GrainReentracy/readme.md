# Projeto GrainReentrancy

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Atributo Reentrant em grains](#3-atributo-reentrant-em-grains)
- [Grains Reentrant em ação](#4-grains-reentrant-em-ação]
- [Sumário](#5-sumário)

# 1. Introdução

Vamos ver como é simples configurar um **Grain** para que seus métodos possam ser executados de forma não linear entre si.

<div align="right">
	
[Voltar](#projeto-grainreentrancy)

</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker][docker-site]. Use a linha de comando que eu separei no repositório [DockerShortcuts][docker-shortcuts].

<div align="right">
	
[Voltar](#projeto-grainreentrancy)

</div>

# 3. Atributo Reentrant em grains

De forma muito simples, basta colocar o atributo `Reentrant` para que o **Grain** possa começar a execução de um método sem necessariamente esperar o término da execução de outro (se estiver sendo executado, claro). Neste exemplo, temos dois **Grains** com exatamente o mesmo código, e apenas o atributo `Reentrant` como diferença.

O **Grain** normal permitirá a execução dos métodos de forma linear na mesma ativação (mesma chave primária). Então se chamarmos vários métodos `Do` e vários métodos `Make`, você nunca verá o início da execução de um dos métodos enquanto o outro não tiver terminado.

```csharp
public class NormalGrain : Grain, INormalGrain
{
	private readonly ILogger<NormalGrain> _logger;

	public NormalGrain(ILogger<NormalGrain> logger)
	{
		_logger = logger;
	}

	public async Task Do()
	{
		_logger.LogInformation("WILL DO IT!");
		await Task.Delay(20);
		_logger.LogInformation("DID IT!");
		
	}

	public async Task Make()
	{
		_logger.LogInformation("WILL MAKE IT!");
		await Task.Delay(10);
		_logger.LogInformation("MADE IT!");
	}
}
```

Já no caso do **Grain** com o atributo `Reentrant`, nós conseguimos ver se um método pode ser executado durante a execução de outro método do mesmo **Grain** (mesma chave primária). Neste caso, o método `Do` pode começar a ser executado mesmo antes do método `Make` ter sua execução encerrada e vice-versa, claro. Vale destacar que isso não significa que os métodos são executados de forma paralela, porque não são. A execução de um **Grain** é em **uma única thread**.

```csharp
[Reentrant]
public class ReentryGrain : Grain, IReentryGrain
{
	private readonly ILogger<IReentryGrain> _logger;

	public ReentryGrain(ILogger<IReentryGrain> logger)
	{
		_logger = logger;
	}

	public async Task Do()
	{
		_logger.LogInformation("WILL DO IT!");
		await Task.Delay(20);
		_logger.LogInformation("DID IT!");
		
	}

	public async Task Make()
	{
		_logger.LogInformation("WILL MAKE IT!");
		await Task.Delay(10);
		_logger.LogInformation("MADE IT!");
	}
}
```

<div align="right">
	
[Voltar](#projeto-grainreentrancy)

</div>

#4. Grains Reentrant em ação

Podemos ver bem a diferença de comportamento dos **Grains** através do código feito no **Client** desde projeto. Basicamente estamos chamando dos métodos `Do` e `Make` 10 vezes em cada grain.

```csharp
// aqui o grain pode ser INormalGrain ou IReentryGrain, ambos são chamados no código
// isso é uma representação

for (var i = 0; i < 10; i++)
{
	listOfTasks.Add(Task.Factory.StartNew(async () =>
	{
		await grain.Do();
	}));
}
for (var i = 0; i < 10; i++)
{
	listOfTasks.Add(Task.Factory.StartNew(async () =>
	{
		await grain.Make();
	}));
}
```

No caso do **Grain** normal você nunca verá os logs misturados dos métodos por eles sempre serão executados de forma linear. Neste caso, o log do **Silo** é algo assim:

```
WILL DO IT!
DID IT!
WILL DO IT!
DID IT!
WILL MAKE IT!
MADE IT!
WILL DO IT!
DID IT!
WILL MAKE IT!
MADE IT!
```

Como dito anteriormente, só vemos a execução de um método após o término de outro - nem se seja uma nova execução do mesmo método. Agora veja como o comportamento do **Grain** com o atributo `reentrant` é diferente:

```
WILL DO IT!
WILL DO IT!
DID IT!
WILL MAKE IT!
DID IT!
MADE IT!
```

Isso não necessariamente acontece todas as vezes, mas o início da execução de um método não depende mais do término de outra execução - podemos ver uma chamada ao método `Do` foi feita antes da chamada anterior ter sido concluída.


# 5. Sumário

- O atributo `Reentrant` permite que métodos do mesmo **Grain** possam ser chamados sem esperar chamadas anteriores.
- Lembre-se que o **Grain** continua sendo executado numa única thread!

<div align="right">
	
[Voltar](#projeto-grainreentrancy)

</div>

[docker-shortcuts]: https://github.com/prrandrade/DockerShortcuts
[docker-site]: https://www.docker.com/