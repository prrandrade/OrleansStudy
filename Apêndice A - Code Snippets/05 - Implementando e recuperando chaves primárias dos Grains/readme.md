# Implementando e recuperando chaves primárias dos Grains



### Chave GUID

```csharp
public interface IGuidGrain : IGrainWithGuidKey
{
	Task<Guid> GetKey();
}

public class GuidGrain : Grain, IGuidGrain
{
	public Task<Guid> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKey());
	}
}
```

### Chave LONG

```csharp
public interface ILongGrain : IGrainWithIntegerKey
{
	Task<long> GetKey();
}

public class LongGrain : Grain, ILongGrain
{
	public Task<long> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKeyLong());
	}
}
```

### Chave STRING

```csharp
public interface IStringGrain : IGrainWithStringKey
{
	Task<string> GetKey();
}

public class StringGrain : Grain, IStringGrain
{
	public Task<string> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKeyString());
	}
}
```

### Chave composta GUID + STRING

```csharp
public interface IGuidAndStringGrain : IGrainWithGuidCompoundKey
{
	Task<Guid> GetKey();

	Task<string> GetSecondaryKey();
}

public class GuidAndStringGrain : Grain, IGuidAndStringGrain
{
	public Task<Guid> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKey(out _));
	}

	public Task<string> GetSecondaryKey()
	{
		this.GetPrimaryKey(out var keyExt);
		return Task.FromResult(keyExt);
	}
}
```

### Chave composta LONG + STRING

```csharp
public interface ILongAndStringGrain : IGrainWithIntegerCompoundKey
{
	Task<long> GetKey();

	Task<string> GetSecondaryKey();
}

public class LongAndStringGrain : Grain, ILongAndStringGrain
{
	public Task<long> GetKey()
	{
		return Task.FromResult(this.GetPrimaryKeyLong(out _));
	}

	public Task<string> GetSecondaryKey()
	{
		this.GetPrimaryKeyLong(out var keyExt);
		return Task.FromResult(keyExt);
	}
}
```