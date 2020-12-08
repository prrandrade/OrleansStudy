# Projeto GrainTimers

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Quando usar Timers](#3-quando-usar-timers)
- [Como usar Timers](#4-como-usar-timers)
- [Timers na prática](#5-timers-na-prática)
- [Sumário](#6-sumário)

# 1. Introdução

Vamos aprender como usar, ativar e desativar **Timers** que os **Grains** podem usar para a realização de tarefas repetitivas.

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker][docker-site]. Use a linha de comando que eu separei no repositório [DockerShortcuts][docker-shortcuts].

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 3. Quando usar Timers

**Timers** são tarefas agendadas para serem executadas de forma contínua enquanto estiverem ativas OU enquanto o **Grain** estiver ativo. Ou seja, se o **Grain** for desativado, o timer também o será! **Timers** também são mais simples em termos de processamento e não precisam de nenhuma configuração extra.

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 4. Como usar Timers

Neste projeto de exemplo, o **Timer** também receberá um objeto como parâmetro (isso é opcional e no máximo **UM** objeto pode ser passado como parâmetro). A variável que representa o **Timer** no **Grain** é simplesmente do tipo `IDisposable`, pois realmente não precisamos de nenhum método específico de **Timers** para manipulação.

```csharp
private IDisposable _timer;
private readonly ExampleModel _teste = new ExampleModel { Value = 0 };
```

No método `ActivateTimer` do **Grain** estamos ativando o **Timer** (e checando se ele já não está ativado, senão teremos dois **Timers** sendo executados simultaneamente!). Note que estamos usando o método `RegisterTimer` para criar o **Timer** em questão, e ele recebe quatro parâmetros:

- Uma função que recebe o objeto como parâmetro e devolve uma `Task`. É esta `Task` que será executada continuamente.
- O objeto que será enviado ao **Timer** como parâmetro (neste caso, `_teste`). Se não houver objeto a ser passado, este valor deve ser `null`.
- Um `TimeSpan` que representa quanto tempo passará até a **primeira execução** do **Timer** - neste caso, 5 segundos.
- Um `TimeSpan` que representa quanto tempo passará **entre cada execução após a primeira execução** - neste caso, 2 segundos.

```csharp
public Task ActivateTimer()
{
	if (_timer == null)
	{
		_timer = RegisterTimer(obj =>
		{
			return Task.Factory.StartNew(() =>
			{
				Console.WriteLine($"Timer called and the current value of {((ExampleModel)obj).Value}");
				((ExampleModel)obj).Value++;
			});
		}, _teste, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2));
	}
	else
	{
		Console.WriteLine("Timer is already activated!");
	}

	return Task.CompletedTask;
}
```

No método `DeactivateTimer` validamos se o `_timer` é `null`. Caso não seja, simplesmente damos um `Dispose` nele e marcamos a variável como `null`. Isso já desativa o **Timer**.

```csharp
public Task DeactivateTimer()
{
	if (_timer == null)
	{
		Console.WriteLine("Timer is already deactivated!");
	}
	else
	{
		_timer?.Dispose();
		_timer = null;
		Console.WriteLine("Timer deactivated.");
	}
	return Task.CompletedTask;
}
```

Só que se o **Grain** for desativado, o **Timer** também o será. E é isso que o método `DeactivateGrain` faz, ao chamar o método interno `DeactivateOnIdle`. Lembre-se que a desativação do **Grain** faz com que todas as informações não persistidas sejam descartadas - ou seja, neste caso, o contador dentro do objeto `_teste` também será descartado.

```csharp
public Task DeactivateGrain()
{
	DeactivateOnIdle();
	return Task.CompletedTask;
}
```

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 5. Timers na prática

No **Client**, temos um pequeno roteiro que mostra na prática os comportamentos citados na ativação. Por exemplo, o código exibido logo abaixo ativa o **Timer**, o que fará com que o **Silo** mostre o contador do **Grain** sendo alterado.

```csharp
// aqui o timer é ativado
await grain.ActivateTimer();
Console.ReadKey(true);
```

Se tentarmos ativar o **Timer** novamente, nada ocorre, porque estamos fazendo a checagem para não ter dois **Timers** simultâneos.

```csharp
// aqui o timer não será mais ativado, pois já o ativamos
await grain.ActivateTimer();
Console.ReadKey(true);
```

Mesmo procedimento na hora de desativar o **Timer**. A primeira chamada faz a desativação, e a segunda chamada não faz nada, porque estamos checando se o **Timer** já está ativado.

```csharp
// aqui o timer será desativado
await grain.DeactivateTimer();
Console.ReadKey(true);

// aqui o timer não será desativado, pois já o desativamos
await grain.DeactivateTimer();
Console.ReadKey(true);
```

Agora vem que a parte interessante. Nós ativamos o **Timer** mais uma vez. O contador que o **Timer** modifica não foi zerado porque o **Grain** não foi desativado. Mas se desativarmos o **Grain** nem precisamos desativar o **Timer** - este já será zerado automaticamente.

```csharp
// aqui o timer é ativado novamente
await grain.ActivateTimer();
Console.ReadKey(true);

// aqui o grain é desativado, o que desativa o timer
await grain.DeactivateGrain();
Console.ReadKey(true);
```

E se ativarmos o **Timer** mais uma vez (o que já garante a ativação do **Grain**), o contador começará do zero, porque o **Grain** havia sido desativado anteriormente e não persistimos o objeto manipulado pelo **Timer**.

```csharp
// aqui o timer será ativado (e o grain reativado)
await grain.ActivateTimer();
Console.ReadKey(true);
```

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 6. Sumário

- **Timers** não precisam de configuração extra e podem ser usados para tarefas corriqueiras e/ou bem frequentes.
- **Timers** ficam armazenados em memória, eles são zerados quando o **Grain** é desativado no **Silo**.

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

[docker-site]: https://www.docker.com/
[docker-shortcuts]: https://github.com/prrandrade/DockerShortcuts
