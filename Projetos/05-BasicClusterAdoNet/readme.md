# Projeto BasicClusterAdoNet

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Pacotes que precisam ser instalados no Silo](#3-pacotes-que-precisam-ser-instalados-no-silo)
- [Configurações no Silo](#4-configurações-no-silo)
- [Pacotes que precisam ser instalados no Client](#5-pacotes-que-precisam-ser-instalados-no-client)
- [Configurações no Client](#6-configurações-no-client)
- [Sumário](#7-sumário)

# 1. Introdução

Agora que já sabemos a estruturação de um projeto Orleans e entendemos a teoria de um **Cluster**, vamos aprender como configurar um projeto que depende uma base de dados para organização do **Cluster**.

<div align="right">
	
[Voltar](#projeto-basicclusteradonet)

</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker](https://www.docker.com). Use a linha de comando que eu separei no repositório [DockerShortcuts](https://github.com/prrandrade/DockerShortcuts).

<div align="right">
	
[Voltar](#projeto-basicclusteradonet)

</div>

# 3. Pacotes que precisam ser instalados no Silo

Vale relembrar que você precisa instalar alguns pacotes nuget extras no **Silo** quando estamos usando uma base de dados ADO.NET para a organização do **Cluster** e da persistência em geral:

- Para SQL Server: **System.Data.SqlClient**
- Para MySql/MariaDB: **MySql.Data**
- Para PostgreSQL: **Npgsql**
- Para Oracle: **ODP.net**

Também precisamos baixar pacotes separados para a configuração de clusterização, de persistência e de tarefas agendadas:

- Para clusterização: **Microsoft.Orleans.Clustering.AdoNet**
- Para tarefas agendadas: **Microsoft.Orleans.Reminders.AdoNet**
- Para persistência: **Microsoft.Orleans.Persistence.AdoNet**

Neste projeto de **Silo**, vamos usar o SQL Server como mecanismo para clusterização, tarefas agendadas e persistência, então os pacotes instalados são:

- Microsoft.Orleans.Server
- Microsoft.Extensions.Logging.Console
- System.Data.SqlClient
- Microsoft.Orleans.Clustering.AdoNet
- Microsoft.Orleans.Reminders.AdoNet
- Microsoft.Orleans.Persistence.AdoNet

<div align="right">
	
[Voltar](#projeto-basicclusteradonet)

</div>

# 4. Configurações no Silo

Desde o [HelloWorld][helloworld], sabemos que é a classe `SiloHostBuilder` que realmente faz o **Silo** ser.. bem, um **Silo**. E originalmente usamos o método `UseLocalhostClustering` para executar o **Silo** em ambiente local sem nenhuma configuração.

O que este método faz é simples: cria um **Silo** com configuração de clusterização própria para ambiente de desenvolvimento, ouvindo as portas padrão de comunicação entre **Client** e **Silo** (porta 11111 entre **Silos** e porta 30000 entre **Client** e **Silo**) e usando os nomes padrão de serviço e cluster (*dev*). Agora precisamos aplicar estas configurações manualmente.

A configuração de portas pode ser realizada através do método `ConfigureEndpoints`. Simplesmente escolhemos quais portas serão usadas para a comunicação entre **Silos** (parâmetro `siloPort`) e para a comunicação entre **Silo** e **Client** (parâmetro `gatewayPort`).

```
ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)
```

Já a configuração do nome de serviço e nome de cluster devem ser feitas através do método `.Configure<ClusterOptions>`. Mas primeiramente precisamos entender a diferença entre o nome do serviço e o nome do Cluster.

O nome do serviço - `ServiceId` - deve ser único entre todos os **Silos** e não deve mudar. Ele serve como identificação única de todo o conjunto de **Silos** que formam o **Cluster** e também serve de identificação para a persistência de objetos - o mesmo **Silo** com um nome de serviço diferente não consegue carregar um objeto persistido anteriormente com outro nome de serviço.

Já o nome do cluster - `ClusterId` - pode ser alterado sem problemas, mas vale destacar que apenas os **Silos** com o mesmo `ClusterId` conversam entre si. Isso pode ser usado de forma inteligente entre deploys para que **Silos** mais recentes conversam apenas entre si durante o processo de atualização - uma abordagem prática do processo [blue-green de deployment][bluegreen]. Se você não precisa deste tipo de abordagem, pode deixar o nome do cluster igual ao nome do serviço.

```csharp
Configure<ClusterOptions>(options =>
{
	options.ClusterId = "dev";
	options.ServiceId = "dev";
})
```

A configuração de clusterização, tarefas agendadas e persistência de objetos é feita respectivamente pelos métodos `UseAdoNetClustering`, `UseAdoNetReminderService` e `AddAdoNetGrainStorage`. Os três métodos tem as mesmas propriedades: `ConnectonString`, que armazena a string de conexão da base de dados e `Invariant`, que identifica o tipo de base de dados que será usada. Vale destacar que as identificações e as bases compatíveis já foram [listadas anteriormente][readme-parte2]. Note também que, como as configurações são separadas, podemos usar bases de dados diferentes para cada recurso.

```csharp
UseAdoNetClustering(options =>
{
	options.Invariant = "System.Data.SqlClient";
	options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
})

.UseAdoNetReminderService(options =>
{
	options.Invariant = "System.Data.SqlClient";
	options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
})

.AddAdoNetGrainStorage("GrainTable", options =>
{
	options.Invariant = "System.Data.SqlClient";
	options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
})
```

<div align="right">
	
[Voltar](#projeto-basicclusteradonet)

</div>

# 5. Pacotes que precisam ser instalados no Client

Quando um **Client** acessa um **cluster** gerenciado por uma base de dados, ele **deve ter acesso a esta base**. E isso também vale para os pacotes nuget necessários. Você precisa instalar o pacote nuget **Microsoft.Orleans.Clustering.AdoNet** e o pacote de dados referente a base de dados usada para clusterização - neste exemplo específico, é **System.Data.SqlClient**, pois a base usada é o SQL Server.

Resumindo, no projeto **Client**, os seguintes pacotes estão instalados:

- Microsoft.Orleans.Core
- Microsoft.Extensions.Logging.Console
- Microsoft.Orleans.Clustering.AdoNet
- System.Data.SqlClient

<div align="right">
	
[Voltar](#projeto-basicclusteradonet)

</div>

# 6. Configurações no Client

De forma curiosa, mas não surpreendente, as mudanças que precisamos fazer nas configurações do `ClientBuilder` são bem semelhantes. precisamos tirar o uso do método `UseLocalhostClustering`, pois não estamos mais conectando a um **Silo** local, por exemplo, e fazer as configurações manualmente.

Em primeiro lugar, precisamos identificar o `ClusterId` e o `ServiceId` do **Cluster** que será usado pelo **Client**, através do método `Configure<ClusterOptions>()`. Note que o uso do método de configuração é exatamente o mesmo do **Silo**.

```csharp
Configure<ClusterOptions>(options =>
{
	options.ClusterId = "dev";
	options.ServiceId = "dev";
})
```

De forma bastante interessante, não listamos os **Silos** que estão disponíveis no **Cluster**. É a base de dados ADO.NET usada que faz esta comunicação indireta - assim, se um **Silo** novo é criado, não precisamos parar nenhum **Client** para atualizar a listagem de **Silos** disponíveis. A configuração é feita através do método `UseAdoNetClustering` é exatamente a mesma do **Silo** - tanto a string de conexão na propriedade `ConnectionString` quando a identificação da base de dados na propriedade `Invariant`. Os valores para esta propriedade já foram [explicados anteriormente][readme-parte2].

```csharp
.UseAdoNetClustering(options =>
{
	options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
	options.Invariant = "System.Data.SqlClient";
})
```

O mais legal disso tudo é que o **Client** nem sabe (via código) onde estão os **Silos**. É tudo feito via as informações na base de dados! O Orleans faz o meio de campo sempre garantindo que o **Silo** mais ocioso do **Cluster** ative o **Grain** e execute seus métodos, quando for chamado!

<div align="right">
	
[Voltar](#projeto-basicclusteradonet)

</div>

# 7. Sumário

- Num ambiente corporativo, a organização entre **Silos** e **Clients** pode ser feita através de bases de dados.
- Diferentes bases de dados podem ser usadas para clusterização, tarefas agendadas e persistência de objetos. Você não precisa mexer nas bases de dados já existentes e nem precisa usar a mesma base de dados para as três funcionalidades - mas lembre-se de [executar os scripts de preparação das bases][readme-parte2]!
- **Clients** não sabem onde estão os **Silos** e nem precisam se conectar a eles. Todo este meio de campo é feito através da base de dados de clusterização, o que é uma das vantagens do Orleans. Você consegue subir mais **Silos** no mesmo **Cluster** sem nem precisar reiniciar **Clients**!

<div align="right">
	
[Voltar](#projeto-basicclusteradonet)

</div>

[bluegreen]: https://martinfowler.com/bliki/BlueGreenDeployment.html
[readme-parte2]: https://github.com/prrandrade/OrleansStudy/tree/master/Parte%202%20-%20Computa%C3%A7%C3%A3o%20distribu%C3%ADda%20e%20persist%C3%AAncia%20com%20o%20Orleans
[helloworld]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/01-HelloWorld
