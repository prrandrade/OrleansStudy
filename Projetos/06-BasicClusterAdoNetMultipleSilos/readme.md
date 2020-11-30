# Projeto BasicClusterAdoNetMultipleSilos

- [Introdução](#introdução)
- [Observação rápida sobre a base de dados](#observação-rápida-sobre-a-base-de-dados)
- [Executando mais de um Silo na mesma máquina](#executando-mais-de-um-silo-na-mesma-máquina)
- [Como funciona o registro de portas dos Silos](#como-funciona-o-registro-de-portas-dos-silos)
- [Pacotes que precisam ser instalados no Client](#pacotes-que-precisam-ser-instalados-no-client)
- [Grain deste projeto](#grain-deste-projeto)
- [Client usando mais de um Silo sem perceber](#client-usando-mais-de-um-silo-sem-perceber)
- [Lidando com chamadas não respondidas entre Client e Silo](#lidando-com-chamadas-não-respondidas-entre-client-e-silo)
- [Sumário](#sumário)

# Introdução

Vamos aprender como executar mais de um **Silo** na mesma máquina e ver como o acesso de um Client a um **Grain** é distribuído entre os **Silos** sem precisar de nenhuma configuração especial.

# Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker][docker-site]. Use a linha de comando que eu separei no repositório [DockerShortcuts][docker-shortcuts].

# Executando mais de um Silo na mesma máquina

No [projeto BasicClusterAdoNet][05-BasicClusterAdoNet], fizemos a configuração usando uma base de dados SQL Server para organizar a clusterização. Este projeto tem o mesmo ponto de partida - o que inclui [aplicar os scripts de preparação][readme-parte2], lembre-se disso! Mas há uma diferença fundamental neste caso. Veja, quando nós configuramos o **Silo**, precisamos separar duas portas de rede - a porta de comunicação entre **Clients** e o **Silo**, chamada de `gatewayPort` , e a porta de comunicação entre os **Silos**, chamada de **SiloPort**.

Normalmente esta configuração é a mesma para todos os **Silos**, porque cada **Silo** deve ser executado  numa máquina separada: estamos falando de computação distribuída e não estamos entrando no mérito de executar **Silos** em diferentes máquinas físicas, máquinas virtuais ou kubernetes.

Então, para que os **Silos** possam ser executados na mesma máquina, precisamos configurar para que cada **Silo** use uma porta de comunicação com **Client** e com os outros **Silos** diferente.

Como o **Silo** é uma Console Application, vamos simplesmente passar as portas como parâmetro. Desta forma, podemos executar um **Silo** ouvindo as portas 11111 e 30000 e outro **Silo** na mesma máquina ouvindo as portas 11112 e 30001, por exemplo - conseguimos sem muito esforço executar múltiplos Silos na mesma máquina.

No exemplo acima, executamos os comandos `Silo.exe 11111 30000` par executar o primeiro **Silo** e `Silo.exe 11112 30001` para executar o segundo **Silo**. Mas como o Orleans organiza isso?

```csharp
int siloPort;
int gatewayPort;

try
{
    siloPort = Convert.ToInt32(Environment.GetCommandLineArgs()[1]);
    gatewayPort = Convert.ToInt32(Environment.GetCommandLineArgs()[2]);
}
catch
{
    siloPort = 11111;
    gatewayPort = 30000;
}
```

De resto, a configuração do **Silo** é a mesma que já vimos anteriormente. Note que, neste caso, estamos aplicando apenas a configuração de clusterização, é apenas ela que será usada no projeto.

```csharp
// configurando acesso ao silo
.Configure<ClusterOptions>(options =>
{
	options.ClusterId = "dev";
    options.ServiceId = "dev";
})
.ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort)

// clustering organizado via banco de dados
.UseAdoNetClustering(options =>
{
	options.Invariant = "System.Data.SqlClient";
    options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
})

.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
.ConfigureLogging(logging => logging.AddConsole());
```

# Como funciona o registro de portas dos Silos

O banco de dados deve ser usado para a organização de clusterização porque é nele que as informações de conexão dos **Silos** são armazenadas, incluindo portas, são armazenadas e utilizadas para a correta comunicação **Client** - **Silo** e **Silo** - **Silo**. A tabela `OrleansMembershipVersionTable` apresenta informações sobre o **Cluster**. Neste caso, é algo parecido com:

```
DeploymentId	Timestamp					Version
dev				2020-11-30 15:18:11.687		4
```

Enquanto isso, a tabela `OrleansMembershipTable` mostra informações dos **Silos** - incluindo as portas que cada um está utilizando.  E exemplo abaixo propositalmente não inclui algumas informações da tabela:

```
DeploymentId	Address			Port	SiloName	Status	ProxyPort	StartTime				IAmAliveTime
dev				32.94.156.189	11111	Silo_03eec	3		30000		2020-11-30 15:17:47.093	2020-11-30 15:22:47.513
dev				32.94.156.189	11112	Silo_1c544	3		30001		2020-11-30 15:18:11.267	2020-11-30 15:18:11.690
```

Num ambiente de desenvolvimento, apagar o **conteúdo** destas tabelas é a forma mais fácil de recriar do zero um **Cluster**.

# Grain deste projeto

O **Grain** deste projeto é propositalmente simples, seu objetivo é escrever um log para mostrar qual **Silo** o executou.

```csharp
public class HelloGrain : Grain, IHelloGrain
{
	private readonly ILogger<HelloGrain> _logger;

	public HelloGrain(ILogger<HelloGrain> logger)
	{
		_logger = logger;
	}

	public Task Ping()
	{
		_logger.LogInformation($"Pinged {this.GetPrimaryKeyLong()}!");
		return Task.CompletedTask;
	}
}
```

# Client usando mais de um Silo sem perceber

O **Client** neste projeto ativa 10000 **Grains** (obviamente com 10000 chaves primárias diferentes) de forma assíncrona e chama o método `Ping()`. Ao término, recebemos o número de chamadas respondidas. Ao executar este código, você vai perceber que ambos os **Silos** recebem chamadas do **Client**, sem nenhuma configuração necessária!

```csharp
for (var i = 0; i < 10000; i++)
{
	var j = i;
	listOfTasks.Add(Task.Factory.StartNew(() =>
	{
		var friend = client.GetGrain<IHelloGrain>(j);
		try
		{
			friend.Ping().WaitWithThrow(TimeSpan.FromMilliseconds(5000));
			listOfResults.TryAdd(j, true);
			Console.WriteLine($"{j} - Pong!");
		}
		catch
		{
			listOfResults.TryAdd(j, false);
			Console.WriteLine($"{j} - Not Pong!");
		}
	}));
}

Task.WaitAll(listOfTasks.ToArray());
var numberOfpongs = listOfResults.Values.Count(x => x);
Console.WriteLine($"{numberOfpongs} calls where ponged!");
Console.ReadKey();
```

Exemplo de log de um **Silo**:

```
info: Grains.HelloGrain[0]
      Pinged 8140!
info: Grains.HelloGrain[0]
      Pinged 8141!
info: Grains.HelloGrain[0]
      Pinged 8142!
info: Grains.HelloGrain[0]
      Pinged 8137!
```

Exemplo de log no outro **Silo**:

```
info: Grains.HelloGrain[0]
      Pinged 8131!
info: Grains.HelloGrain[0]
      Pinged 8136!
info: Grains.HelloGrain[0]
      Pinged 8138!
info: Grains.HelloGrain[0]
      Pinged 8143!
```

# Lidando com chamadas não respondidas entre Client e Silo

É totalmente possível que a chamada que o **Client** faz para **Silo** não seja respondida em tempo hábil, ou o **Silo** tenha problemas durante a execução do **Grain**. A forma mais fácil do **Client** se proteger contra este tipo de problema é usando o método de extensão  `WaitWithThrow`, presente no pacote `Microsoft.Orleans.Core`, já instalado no **Client**. Vamos ver com mais detalhe o próprio código acima onde este método é usado.

O parâmetro esperado é um `TimeSpan` com o tempo que a thread deve aguardar até que a resposta da chamada do **Grain** seja recebida. Se não houver resposta no tempo passado - neste exemplo são 5 segundos, uma exceção do tipo `TimeoutException` é disparada no **Client**. Assim, você consegue reprogramar o disparo do método de forma automática, por exemplo.

```csharp
friend.Ping().WaitWithThrow(TimeSpan.FromMilliseconds(5000));
```

# Sumário

- Podemos executar mais de um **Silo** na mesma máquina sem problemas, desde que cada **Silo** use portas específicas para comunicação entre si e entre **Clients**.
- Todas as configurações de conexão são feitas apenas nos **Silos** e armazenadas na base de dados escolhida. Justamente por isso o **Client** apenas se conecta apenas à base de dados.
- Não é necessária nenhuma configuração nos **Clients** para distribuir as chamadas dos **Grains** em diferentes **Silos**. A partir que mais de um **Silo** está no mesmo **Cluster**, a computação distribuída já funciona.
- O método ``WaitWithThrow`` permite esperar por um tempo finito pela resposta de uma chamada, permitindo programar outros comportamentos.

[readme-parte2]: https://github.com/prrandrade/OrleansStudy/tree/master/Parte%202%20-%20Computa%C3%A7%C3%A3o%20distribu%C3%ADda%20e%20persist%C3%AAncia%20com%20o%20Orleans
[05-BasicClusterAdoNet]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/05-BasicClusterAdoNet

