# Grains com múltiplas fontes de persistência

```csharp
// objetos persistidos devem ser marcados com o atributo Serializable
[Serializable]
public class ExampleState
{
	public int Value { get; set; }
}
```

```csharp
public class Grain : Grain, IConversationGrain
{
	private readonly IPersistentState<ExampleState> _state;

	// nome único para o objeto persistido no atributo PersitentState
	// identificação do storage já configurado anteriormente no atributo PersistentState
	public ConversationGrain([PersistentState("state", "storage")] IPersistentState<ExampleState> state)
	{
		_state = state;
	}

	public async Task ReadState()
	{
		// le o objeto que está persistido
		await _state.ReadStateAsync();
	}

	public async Task WriteState()
	{
		// objeto modificado fica na propriedade State
		_state.State.Value = 1;
		await _state.WriteStateAsync();
	}

	public async CleanState()
	{
		// limpa o objeto em memória e na persistência
		await _conversationState.ClearStateAsync();
	}	
}
```

