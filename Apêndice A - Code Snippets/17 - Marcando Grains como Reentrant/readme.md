# Marcando Grains como Reentrant

```csharp
[Reentrant]
public class ReentryGrain : Grain, IReentryGrain
{
	// now the method Do can be called before the method Make ends for the same Grain
	public async Task Do()
	{
		await Task.Delay(20);		
	}

	// now the method Make can be called before the method Do ends for the same Grain
	public async Task Make()
	{
		await Task.Delay(10);
	}
}
```