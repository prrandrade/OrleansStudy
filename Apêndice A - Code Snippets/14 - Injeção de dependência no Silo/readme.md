# Injeção de dependência no Silo

```csharp
private static async Task<ISiloHost> StartSilo()
{
	var builder = new SiloHostBuilder()

		// ... outras configurações do SiloBuilder

		.ConfigureServices(options =>
		{
			options.AddScoped<ISomeInterface, SomeImplementation>();
			options.AddTransient<IAnotherInteface, AnotherImplementation>();
			options.AddSingleton<IOneMoreInterface, OneMoreImplementation>();
		})		

	var host = builder.Build();
	await host.StartAsync();
	return host;
}
```