# Projeto PrimaryKeys

- [Introdução](#introdução)
- [Estrutura do Grain](#estrutura-do-grain)
- [Client executando métodos de forma serial](#client-executando-métodos-de-forma-serial)
- [Sumário](#sumário)

# Introdução

Após passar pelo [HelloWorld](https://github.com/prrandrade/OrleansStudy/tree/master/study/01-HelloWorld), não precisamos mais fazer um tour pela estrutura dos projetos; vamos direto ao ponto e entender como o conceito de Virtual Actor simplifica a chamada de métodos.

# Estrutura do Grain

O **Grain** deste exemplo é `ExampleGrain`, que implementa a interface `IExampleGrain`, que por sua vez implementa a interface `IGrainWithIntegerKey` - ou seja, a chave primária é um número inteiro, irrelevante para o exemplo em questão. Enfim, temos o método `Process`, que 'gasta' 3 segundos de processamento e o método `AnoterProcess`, que gasta 5 segundos de processamento.

```csharp
public class ExampleGrain : Grain, IExampleGrain
{
	private readonly ILogger<ExampleGrain> _logger;

	public ExampleGrain(ILogger<ExampleGrain> logger)
	{
		_logger = logger;
	}

	public Task Process()
	{
		_logger.LogInformation($"{nameof(Process)} started at {DateTime.Now}");
		Thread.Sleep(3000);
		return Task.CompletedTask;
	}

	public Task AnotherProcess()
	{
		_logger.LogInformation($"{nameof(AnotherProcess)} started at {DateTime.Now}");
		Thread.Sleep(5000);
		return Task.CompletedTask;
	}
}
```

# Client executando métodos de forma serial

No **Client**, o código que chamada o **Grain** chama os dois métodos em tasks diferentes... e mais ainda, a ativação do **Grain** é realizada nas tasks- note o `client.GetGrain<IExampleGrain>(0)` em ambas as tasks. Em teoria, os métodos `Process` e `AnotherProcess` são executados de forma assíncrona - são tasks diferentes, afinal de contas.

Mas, e este é o pulo do gato, como ambos os **Grains** foram ativados com a MESMA chave primária, para todos os efeitos do Orleans, estamos falando do MESMO **Grain**, e métodos do mesmo **Grain** são executados de forma serial. Literalmente a `task t2` vai ficar esperando que o método `Process` chamado na `task t1` seja executado, se a gente fazer nenhuma configuração!

```csharp
await using var client = await ConnectClient();

var t1 = Task.Factory.StartNew(() =>
{
	var grain = client.GetGrain<IExampleGrain>(0);
	Console.WriteLine($"Grain will run Process at {DateTime.Now:HH:mm:ss.fff}");
	grain.Process().Wait();
	Console.WriteLine($"Grain returned Process at {DateTime.Now:HH:mm:ss.fff}");
});
Thread.Sleep(1000);

var t2 = Task.Factory.StartNew(() =>
{
	var grain = client.GetGrain<IExampleGrain>(0);
	Console.WriteLine($"Grain will run AnotherProcess at {DateTime.Now:HH:mm:ss.fff}");
	grain.AnotherProcess().Wait();
	Console.WriteLine($"Grain return AnotherProcess at {DateTime.Now:HH:mm:ss.fff}");
});

Task.WaitAll(t1, t2);

```

# Sumário

De forma resumida:

- Não importa de onde os **Grains** são ativados, se a chave primária é a mesma, para fins práticos, o **Grain** é o mesmo.

- Métodos do mesmo **Grain** (mesma chave primária) são por definição seriais, precisamos esperar o retorno de um método para conseguir executar outros métodos.

- Métodos de **Grains** com chaves primárias diferentes podem obviamente ser chamados de forma paralela.



