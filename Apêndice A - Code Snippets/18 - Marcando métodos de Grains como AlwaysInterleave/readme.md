# Marcando métodos de Grains como AlwaysInterleave

```csharp
public interface IExampleGrain : IGrainWithIntegerKey
{
	Task DoSlow();

	// marcação do método é feita na interface do grain
	[AlwaysInterleave]
	Task DoFast();
}
```

```csharp
public class ExampleGrain : Grain, IExampleGrain
{
	public async Task DoSlow()
	{
		await Task.Delay(200);
	}
	
	// este método não espera a execução de outros métodos do mesmo grain
	public async Task DoFast()
	{
		await Task.Delay(200);;
	}
}
```