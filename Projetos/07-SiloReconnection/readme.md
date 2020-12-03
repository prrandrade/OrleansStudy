# Projeto SiloReconnection

- [Introdução](#introdução)
- [Observação rápida sobre a base de dados](#observação-rápida-sobre-a-base-de-dados)
- [Grain com métodos com e sem retorno](#grain-com-métodos-com-e-sem-retorno)
- [Formas de se esperar a resposta do Silo](#formas-de-se-esperar-a-resposta-do-silo)
- [Porque não há resposta a tempo no Grain](#porque-não-há-resposta-a-tempo-no-grain)
- [Esperando métodos dos Grains no Client](#esperando-métodos-dos-grains-no-client)
- [Sumário](#sumário)

# Introdução

Vamos aprender como adicionar comportamentos no **Client** para reagir quando uma resposta do **Silo** não chega a tempo ou simplesmente não chega.

# Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker][docker-site]. Use a linha de comando que eu separei no repositório [DockerShortcuts][docker-shortcuts].

# Grain com métodos com e sem retorno

Um **Grain** pode ter quatro tipos de retorno: `Task`, `Task<T>`, `ValueTask` ou `ValueTask<T>`. Os métodos de extensão que o Orleans apresenta para esperar a resposta dos métodos **Grain** valem apenas par `Task` e `Task<T>`. No **Grain** deste projeto, temos dois métodos. O método `CreateLog()` devolve uma `Task` e apenas grava um log no lado do **Silo** (lembre-se, os métodos dos **Grains** são executados no lado do **Silo**). O `Thread.Sleep` de 5 segundos tem como objetivo simular processamento de longa duração:

```csharp
public Task CreateLog()
{
	Thread.Sleep(5000);
	_logger.LogInformation($"Log created for {this.GetPrimaryKeyLong()}!");
	return Task.CompletedTask;
}
```

Já o método `Pow` devolve uma `Task<int>` e faz o cálculo manual de potenciação, sem muito mistério. Aqui também estamos usando alguns `Thread.Sleep` para simular um processamento maior que do normal neste mesmo.

```csharp
public Task<int> Pow(uint a, uint b)
{
	if (b == 0)
		return Task.FromResult(1);

	var r = Convert.ToInt32(a);

	for (var i = 0; i < b; i++)
	{
		Thread.Sleep(500);
		r *= Convert.ToInt32(a);
	}

	return Task.FromResult(r);
}
```

# Formas de se esperar a resposta do Silo

Um **Grain** pode ter quatro tipos de retorno: `Task`, `Task<T>`, `ValueTask` ou `ValueTask<T>`. Os métodos de extensão que o Orleans apresenta para esperar a resposta dos métodos **Grain** valem apenas par `Task` e `Task<T>`. Existem dois métodos de extensão que podem ser usados para esperar o resultado dos métodos do **Grain**:

- `WithTimeout`, que espera o resultado do método por um tempo determinado e dispara uma exceção do tipo `TimeoutException` com uma mensagem personalizada se não houver resposta até o tempo passado.

- `WaitWithThrow`, que espera o resultado do método por um tempo determinado e dispara uma exceção do tipo `TimeoutException` se não houver resposta até o tempo passado (foi o método usado no [projeto anterior][06-BasicClusterAdoNetMultipleSilos]).

Eu sei, os métodos parecem iguais, mas há uma diferença fundamental. O método `WithTimeout` pode ser usado na tarefa juntamente com a palavra `await`, num cenário assíncrono. Já o método `WaitWithThrow` trava a thread de execução esperando o resultado ou a exceção. Outra mudança é que o método `WaitWithThrow` só pode ser usado em `Task`, não em `Task<T>`.

# Porque não há resposta a tempo no Grain

Existem dois casos em que não há resposta hábil a tempo de um método do **Grain** chamado remotamente.

- Simplesmente esperamos menos tempo que o método precisa para ser executado - é o que está sendo forçado neste caso.
- O **Silo** que está processando o **Grain** perdeu a conexão com o resto do **Cluster**. Neste caso, o **Client** não obterá resposta, e tentar o mesmo método novamente fará com que outro **Silo** faça o trabalho.

# Esperando métodos dos Grains no Client

O primeiro exemplo mostra como podemos usar o método `WithTimeout` num método que retorna uma `Task<T>`, inclusive chamando o método com `await`.

```csharp
// método de extensão withTimeout
int resultPow;
try
{
	resultPow = await math
		.Pow(5, 5)
		.WithTimeout(TimeSpan.FromMilliseconds(2000), "Timeout for method Generate!");
}
catch (TimeoutException t)
{
	Console.WriteLine(t);
	Console.WriteLine("Let's try again!");
	resultPow = await math
		.Pow(5, 5)
		.WithTimeout(TimeSpan.FromDays(1));
}
Console.WriteLine($"5^5 = {resultPow}");
```

Como dito anteriormente, o método de extensão `WithTimeout` também pode ser usado em `Tasks`, que não retornam um valor de fato, mas podem ser executados de forma assíncrona (note o `await`).

```csharp
// método de extensão withTimeout
try
{
	await math
		.CreateLog()
		.WithTimeout(TimeSpan.FromMilliseconds(2000), "Timeout for method CreateLog!");
}
catch (TimeoutException t)
{
	Console.WriteLine(t);
	Console.WriteLine("Let's try again!");
	await math.CreateLog().WithTimeout(TimeSpan.FromDays(1));
}
Console.WriteLine("Log was created and returned!");
```

Por último, o método `WaitWithThrow` só pode ser usado em `Tasks` (que não retornam valores). Lembre0-se que este método bloqueia a thread de execução!

```csharp
// método de extensão WaitWithThrow
try
{
	math.CreateLog().WaitWithThrow(TimeSpan.FromMilliseconds(2000));
}
catch (TimeoutException)
{
	Console.WriteLine("Let's try again!");
	math.CreateLog().WaitWithThrow(TimeSpan.FromDays(1));
}
```

Vale destacar que, em todos estes casos, o **Silo** mostrará um alerta de que o método demora mais de 200ms para responder, o que mostra que o Orleans foi pensado por padrão para métodos com resposta rápida!

# Sumário

- Podemos usar métodos de extensão do Orleans, disponíveis no **Client**, para lidar com as ocasiões onde o **Silo** não responde a chamada de um **Grain**.
- O método de extensão `WithTimeout` é mais indicado para não travar a thread principal, se ela está ocupada processando outras chamadas.
- O Orleans parte do pressuposto de que um método de um **Grain** deve devolver a resposta em até 200ms antes de escrever um log de alerta. Isso não bloqueia a execução de nada, claro, mas mostra que o Orleans é pensado para o uso de métodos com resposta rápida.

[readme-parte2]: https://github.com/prrandrade/OrleansStudy/tree/master/Parte%202%20-%20Computa%C3%A7%C3%A3o%20distribu%C3%ADda%20e%20persist%C3%AAncia%20com%20o%20Orleans
[06-BasicClusterAdoNetMultipleSilos]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/06-BasicClusterAdoNetMultipleSilos

