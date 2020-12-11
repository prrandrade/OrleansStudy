# Chamando Grains dentro de outros Grains

```csharp
// grain recebendo via injeção de dependência uma IGrainFactory e chamando outro grain
public class Grain : Grain, IExternalGrain
{
	private readonly IGrainFactory _grainFactory;

	public ExternalGrain(IGrainFactory grainFactory)
	{
		_grainFactory = grainFactory;
	}

	public async Task MakeSomething()
	{		
		var anotherGrain = _grainFactory.GetGrain<IAnotherGrain>(this.GetPrimaryKeyLong());
		return Task.CompletedTask;
	}
}
```