# StatelessGrain

```csharp
// stateless grain, limite de ativação por silo é a quantidade de processadores da máquina
[StatelessWorker]
public class ExampleGrain : Grain, IExampleGrain
{

}
```

```csharp
// stateless grain, com limite fixo de ativações simultâneas
[StatelessWorker(1)]
public class ExampleGrain : Grain, IExampleGrain
{

}
```

