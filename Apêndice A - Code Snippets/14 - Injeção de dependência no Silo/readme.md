# Injeção de dependência no Silo

```csharp
private static async Task<ISiloHost> StartSilo()
{
	var builder = new SiloHostBuilder()

		// ... outras configurações do SiloBuilder

		.ConfigureServices(options =>
		{
			// adição de escopo de dependência
			options.AddScoped<ISomeInterface, SomeImplementation>();
			
			// adição transiente de dependência
			options.AddTransient<IAnotherInteface, AnotherImplementation>();
			
			// adição singleton de dependência
			options.AddSingleton<IOneMoreInterface, OneMoreImplementation>();
		})		

	var host = builder.Build();
	await host.StartAsync();
	return host;
}
```