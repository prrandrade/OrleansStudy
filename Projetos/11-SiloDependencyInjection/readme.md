# Projeto SiloDependencyInjection

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Preparando a base de dados deste projeto](#3-preparando-a-base-de-dados-deste-projeto)
- [Configurando a Injeção de dependência no Silo](#4-configurando-a-injeção-de-dependência-no-silo)
- [Exemplo da injeção de dependência em ação](#5-exemplo-da-injeção-de-dependência-em-ação)
- [Sumário](#6-sumário)

# 1. Introdução

Agora que já sabemos a estruturação de um projeto Orleans e entendemos a teoria de um **Cluster**, vamos aprender como configurar um projeto que depende uma base de dados para organização do **Cluster**.

<div align="right">
	
[Voltar](#projeto-silodependencyinjection)

</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker](https://github.com/prrandrade/DockerShortcuts). Use a linha de comando que eu separei no repositório [DockerShortcuts](https://github.com/prrandrade/DockerShortcuts).

<div align="right">
	
[Voltar](#projeto-silodependencyinjection)

</div>

# 3. Preparando a base de dados deste projeto

Além da base de dados previamente preparada, precisamos de uma segunda base de dados que será usada para a regra de negócio. Use o script abaixo para criar a base e a tabela:

```csharp
create database Business;
GO
use Business;

CREATE TABLE Users (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Identification varchar(255),
    Name varchar(255)
);
```

<div align="right">
	
[Voltar](#projeto-silodependencyinjection)

</div>

# 4. Configurando a Injeção de dependência no Silo

Pense no **Silo**, conceitualmente falando, como a única aplicação que é executada - 'única' de forma bem relativa, porque o **Silo** executa os **Grains**. Portanto, o **Silo** também é o maestro da injeção de dependência dos **Grains**. Na configuração do `SiloHostBuilder`, podemos usar o método `ConfigureServices` para configurar as injeções de dependência, sem nenhum mistério.

De forma simples, você encontrará os métodos de cadastro das interfaces e implementações (não é o foco desta documentação, porém). Veja o exemplo abaixo, onde a interface e implementação de um repositório é feita.

```csharp
.ConfigureServices(options =>
{
	options.AddScoped<IUserRepository, UserRepository>();
})
```

A interface `IUserRepository` apresenta alguns métodos assíncronos para adicionar e resgatar informações da base de dados que criamos anteriormente.

```csharp
public interface IUserRepository
{
	public Task AddAsync(string identification, string name);

	public Task<UserModel> RetrieveAsync(string identification);

	public Task<bool> CheckIfExistsAsync(string identification);
}
```

Já a implementação `UserRepository` faz o acesso direto à base de dados. Para simplificar, estou usando o [Dapper](https://github.com/StackExchange/Dapper) para a abstração de acesso a dados. Repare também que a base de dados usada é diferente da base de dados usada pela clusterização do Orleans no projeto.

```csharp
public class UserRepository : IUserRepository
{
	public async Task AddAsync(string identification, string name)
	{
		await using var connection = new SqlConnection("Server=localhost;Database=Business;User Id=sa;Password=root@1234");
		await connection.ExecuteAsync("insert into users (identification, name) values (@identification, @name)", new { identification, name });
		await connection.CloseAsync();
	}

	public async Task<UserModel> RetrieveAsync(string identification)
	{
		await using var connection = new SqlConnection("Server=localhost;Database=Business;User Id=sa;Password=root@1234");
		var result = await connection.QueryAsync<UserModel>("select * from users where identification = @identification", new { identification });
		await connection.CloseAsync();
		return result.FirstOrDefault();
	}

	public async Task<bool> CheckIfExistsAsync(string identification)
	{
		await using var connection = new SqlConnection("Server=localhost;Database=Business;User Id=sa;Password=root@1234");
		return await connection.ExecuteScalarAsync<bool>("select count(*) from users where identification = @identification", new { identification });
	}
}
```

Por fim, o **Grain** recebe a dependência do repositório via construtor, sem nenhum mistério. A regra de negócio, no caso, está no **Grain**, usando o repositório para acessar a base de dados conforme necessário.

```csharp
public class UserGrain : Grain, IUserGrain
{
	private readonly IUserRepository _repository;

	public UserGrain(IUserRepository repository)
	{
		_repository = repository;
	}

	public async Task<bool> RegisterUser(string name)
	{
		if (await _repository.CheckIfExistsAsync(this.GetPrimaryKeyString()))
			return false;

		await _repository.AddAsync(this.GetPrimaryKeyString(), name);
		return true;
	}

	public async Task<UserModel> RetrieveUser()
	{
		return await _repository.RetrieveAsync(this.GetPrimaryKeyString());
	}
}
```

<div align="right">
	
[Voltar](#projeto-silodependencyinjection)

</div>

# 5. Exemplo da injeção de dependência em ação

O **Client** obviamente não precisa saber o que está acontecendo no **Silo** e nos **Grains**, ele apenas chama os métodos e recebem as respostas dos **Grains**. O trecho abaixo, por exemplo, cadastra um novo cliente com base na chave primária.

```csharp
// usuário com identificação 12345
var user1 = client.GetGrain<IUserGrain>("12345");
var result1 = await user1.RegisterUser("Fulano");
if (result1)
{
	var user1Result = await user1.RetrieveUser();
	Console.WriteLine($"{user1Result.Id} - {user1Result.Identification} - {user1Result.Name}");
}
```

Obviamente, quando ativamos e usamos os mesmos métodos com outra chave primária, mais um usuário é cadastrado e recuperado.

```csharp
// usuário com identificação 54321 
var user2 = client.GetGrain<IUserGrain>("54321");
var result2 = await user2.RegisterUser("Beltrano");
if (result2)
{
	var user2Result = await user2.RetrieveUser();
	Console.WriteLine($"{user2Result.Id} - {user2Result.Identification} - {user2Result.Name}");
}
```

Dada a regra de negócio aplicada no **Grain**, se tentarmos recuperar as informações de um usuário que não foi cadastrado, a resposta do método do **Grain** é nula.

```csharp
// usuário com identificação 11111
var user3 = client.GetGrain<IUserGrain>("11111");
var user3Result = await user3.RetrieveUser();
if (user3Result == null)
	Console.WriteLine("Nenhum usuário com identificação 11111");
```

De forma semelhante (regra de negócio do **Grain**), se tentarmos recadastrar um usuário que já foi cadastrado, recebemos uma resposta negativa.

```csharp
// usuário com identificação 12345 novamente
var user4 = client.GetGrain<IUserGrain>("12345");
var result4 = await user4.RegisterUser("Outro nome");
if (!result4)
	Console.WriteLine("Usuário 12345 já foi registrado!");
```

<div align="right">
	
[Voltar](#projeto-silodependencyinjection)

</div>

# 6. Sumário

- Sem mistério, o **Silo** permite cadastrar de forma padrão as injeções de dependência necessárias para os **Grains**.
- **Clients**, como esperado, não precisam saber sobre a arquitetura dos **Silos** e **Grains** para funcionarem corretamente.

<div align="right">
	
[Voltar](#projeto-silodependencyinjection)

</div>