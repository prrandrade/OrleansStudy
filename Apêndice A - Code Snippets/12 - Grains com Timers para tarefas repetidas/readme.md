# Grains com Timers para tarefas repetidas

```csharp
// um (e apenas um) objeto qualquer pode ser passado para o timer
public class ExampleModel
{
	public int Value { get; set; }
}
```

```csharp
public class ExampleGrain : Grain, IExampleGrain
{
	private IDisposable _timer;
	private readonly ExampleModel _teste = new ExampleModel { Value = 0 };

	public Task ActivateTimer()
	{
		// timer demorará 5 segundos para ser chamado pela primeira vez
		// e depois será executado a cada 2 segundos
		// tendo uma instância do objeto ExampleModel como parâmetro
		// parâmetro de objeto pode ser null
	
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

	public Task DeactivateGrain()
	{
		// quando o grain é desativado, todos os timers também o são
		DeactivateOnIdle();
		return Task.CompletedTask;
	}

	public Task DeactivateTimer()
	{
		// dispose do timer é suficiente para ele deixar de existir
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
}
```

