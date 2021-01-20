# Grains com Reminders para tarefas repetidas

```csharp
// interface do grain precisa implementar IRemindable
public interface IExampleGrain : IGrainWithIntegerKey, IRemindable
{
	Task ActivateReminder();

	Task DeactivateGrain();

	Task DeactivateReminder();
}
```

```csharp
public class ExampleGrain : Grain, IExampleGrain
{
	private int _value;

	public async Task ActivateReminder()
	{
		// tempo mínimo de registro entre execuções de um reminder é de 1 minuto
		await RegisterOrUpdateReminder("reminder1", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60));
	}

	public Task DeactivateGrain()
	{
		// desativar o grain NÃO invalida o reminder 
		DeactivateOnIdle();
		return Task.CompletedTask;
	}

	public async Task DeactivateReminder()
	{
		// obtemos a referência ao reminder pelo nome para desativá-lo
		var reminder = await GetReminder("reminder1");
		if (reminder != null)
			await UnregisterReminder(reminder);
	}

	// este método é executado para todos os reminders do Grain
	// usamos a identificação para saber qual reminder está sendo executado
	public async Task ReceiveReminder(string reminderName, TickStatus status)
	{
		if (reminderName == "reminder1")
		{
			await Task.Factory.StartNew(() =>
			{
				_value++;
				Console.WriteLine($"{status.CurrentTickTime:HH:mm:ss.ffff} - New value is {_value}");
			});
		}
	}
}
```

