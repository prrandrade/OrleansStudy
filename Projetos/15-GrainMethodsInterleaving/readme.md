# Projeto GrainMethodsInterleaving

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Atributo AlwaysInterleave em métodos de grains](#3-atributo-alwaysinterleave-em-métodos-de-grains)
- [Comportamento de métodos AlwaysInterleave em Grains](#4-comportamento-de-métodos-alwaysinterleave-em-grains)
- [Sumário](#5-sumário)

# 1. Introdução

Vamos ver como é simples configurar métodos individuais dos **Grains** para que possam ser chamados individualmente sem esperar o retorno de outros métodos.

<div align="right">
	
[Voltar](#projeto-grainmethodsinterleaving)

</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker][docker-site]. Use a linha de comando que eu separei no repositório [DockerShortcuts][docker-shortcuts].

<div align="right">
	
[Voltar](#projeto-grainmethodsinterleaving)

</div>

# 3. Atributo AlwaysInterleave em métodos de grains

Já sabemos [como o atributo `Reentrant`](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/14-GrainReentrancy) pode ser usado para que um **Grain** possa iniciar chamadas de métodos sem esperar o término de chamadas anteriores (quebrando propositalmente o conceito de Virtual Actor). Só que podemos também quebrar esta regra apenas em métodos específicos, com o atributo `AlwaysInterleave`.

O pulo do gato é que o atributo `AlwaysInterleave` deve ser posto na **Interface** do **Grain**, não em sua implementação. Com isso, o método com o atributo não espera o retorno de outras chamadas do mesmo **Grain** (mesma chave primária) para ser executado.

```csharp
public interface IExampleGrain : IGrainWithIntegerKey
{
	Task DoSlow();

	[AlwaysInterleave]
	Task DoFast();
}
```

```csharp
public class ExampleGrain : Grain, IExampleGrain
{
	private readonly ILogger<ExampleGrain> _logger;

	public ExampleGrain(ILogger<ExampleGrain> logger)
	{
		_logger = logger;
	}

	public async Task DoSlow()
	{
		_logger.LogInformation("WILL DO IT SLOW!");
		await Task.Delay(200);
		_logger.LogInformation("DID IT SLOW!");
	}

	public async Task DoFast()
	{
		_logger.LogInformation("WILL DO IT FAST!");
		await Task.Delay(200);
		_logger.LogInformation("DID IT FAST!");
	}
}
```

<div align="right">
	
[Voltar](#projeto-grainmethodsinterleaving)

</div>

# 4. Comportamento de métodos AlwaysInterleave em Grains

É muito simples ver a diferença de comportamento entre métodos normais de um **Grain** e métodos com o atributo `AlwaysInterleave`. Preste atenção no exemplo abaixo, que está no **Client** do projeto. Basicamente, o mesmo **Grain** chama ambos os métodos de forma assíncrona: 3 vezes o método `DoFast` (que tem o atributo `AlwaysInterleave`), 3 vezes o método `DoSlow` e outras 3 vezes o método `DoFast`.

```csharp
var listOfTasks = new List<Task>();
for (var i = 0; i < 3; i++)
{
	listOfTasks.Add(Task.Factory.StartNew(async () =>
	{
		await grain.DoFast();
	}));
}
for (var i = 0; i < 3; i++)
{
	listOfTasks.Add(Task.Factory.StartNew(async () =>
	{
		await grain.DoSlow();
	}));
}
for (var i = 0; i < 3; i++)
{
	listOfTasks.Add(Task.Factory.StartNew(async () =>
	{
		await grain.DoFast();
	}));
}
Task.WaitAll(listOfTasks.ToArray());
```

Se olharmos o resultado da execução no log do **Silo**, vemos claramente que todas as chamadas ao método `DoSlow` são executadas de forma linear (como esperado, seguindo a lógica do Virtual Actor). e todas as chamadas ao método `DoFast` são feitas sem esperar o término uma das outras e sem esperar o retorno das chamadas `DoSlow`, devido ao atributo `AlwaysInterleave`.

```
WILL DO IT FAST!
WILL DO IT FAST!
WILL DO IT SLOW!
WILL DO IT FAST!
WILL DO IT FAST!
WILL DO IT FAST!
WILL DO IT FAST!
DID IT FAST!
DID IT FAST!
DID IT FAST!
DID IT SLOW!
DID IT FAST!
DID IT FAST!
DID IT FAST!
WILL DO IT SLOW!
DID IT SLOW!
WILL DO IT SLOW!
DID IT SLOW!
```

<div align="right">
	
[Voltar](#projeto-grainmethodsinterleaving)

</div>

# 5. Sumário

- O atributo `AlwaysInterleave` permite que métodos individuais do mesmo **Grain** possam ser chamados sem esperar chamadas anteriores.
- O atributo deve ser marcado na interface do **Grain**, a implementação nem precisa ser alterada.

<div align="right">
	
[Voltar](#projeto-grainmethodsinterleaving)

</div>

[docker-shortcuts]: https://github.com/prrandrade/DockerShortcuts
[docker-site]: https://www.docker.com/