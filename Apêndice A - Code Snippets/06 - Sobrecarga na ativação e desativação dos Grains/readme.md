```csharp
public interface IExampleGrain : IGrainWithIntegerKey
{
	Task DeactivateGrainNow();
}
```

```csharp
public class ExampleGrain : Grain, IExampleGrain
{
	public override Task OnActivateAsync()
	{
		// sobrecarga na ativação do grain
		return base.OnActivateAsync();
	}

	public override Task OnDeactivateAsync()
	{
		// sobrecarga na desativação do grain
		return base.OnDeactivateAsync();
	}
	
	public Task DeactivateGrainNow()
	{
		// este método desativa o Grain assim que possível
		DeactivateOnIdle();
		return Task.CompletedTask;
	}
}
```