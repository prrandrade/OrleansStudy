# Projeto ObjectPersistenceAdoNet

- [Introdução](#introdução)
- [Observação rápida sobre a base de dados](#observação-rápida-sobre-a-base-de-dados)
- [Preparando o Silo para persistência de objetos](#preparando-o-silo-para-a-persistência-de-objetos)
- [Preparando o Grain para a persistência de objetos](#preparando-o-grain-para-a-persistência-de-objetos)
- [Lendo, gravando e apagando objetos persistidos](#lendo,-gravando-e-apagando-objetos-persistidos)
- [Exemplificando a persistência de objetos no Client](#exemplificando-a-persistência-de-objetos-no-client)
- [Sumário](#sumário)

# Introdução

Aqui vamos entender como funciona a persistência de objetos do Orleans usando uma base de dados ADO.NET.

# Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker][docker-site]. Use a linha de comando que eu separei no repositório [DockerShortcuts][docker-shortcuts].

# Preparando o Silo para persistência de objetos

Em primeiro lugar, não podemos confundir a persistência de objetos do Orleans com a persistência de dados já usada em praticamente todos os sistemas - tanto que nem precisamos alterar a base de dados já usada num projeto, quando o objetivo é a migração para o Orleans, como já explicado no [projeto BasicClusterAdoNet][05-BasicClusterAdoNet]. Pense na persistência de dados do Orleans como uma espécie de gerenciamento de estado dos **Grains** - porque não estamos persistindo dados com regras de negócio, estamos persistindo objetos - literalmente, serializando e desserializando objetos - nada de entidades, relacionados, chave primária, etc.

Já falamos sobre isso no [projeto BasicClusterAdoNet][05-BasicClusterAdoNet], mas vale a pena falar com mais detalhes aqui. Já sabemos que  são os **Silos** que executam os **Grains**, então são os **Silos** que precisam se preparar para a persistência de objetos. Em primeiro lugar, a base de dados de persistência precisa estar previamente preparada com os scripts já [descritos aqui][05-BasicClusterAdoNet]. Além disso, o pacote `Microsoft.Orleans.Persistence.AdoNet` e o pacote da base de dados em questão precisar estar instalados.

Nas configurações do `SiloHostBuilder`, simplesmente adicionamos o método `AddAdoNetGrainStorage` com a string de conexão a base de dados (`ConnectionString`) e a identificação do tipo de base de dados que será usada para persistência (`Invariant`) e sim, é um `string` que identifica a base de dados.

```csharp
// persistência de objetos organizada via banco de dados
.AddAdoNetGrainStorage("GrainTable", options =>
{
	options.Invariant = "System.Data.SqlClient";
	options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
})
```

Só que, como explicado anteriormente, a persistência de objetos funciona através da serialização dos mesmos. E por definição, é utilizada a serialização binária do .NET. Para que os objetos sejam serializados como JSON, marque a propriedade `UseJsonFormat` como `true`. Com isso, você pode configurar vários aspectos de como os objetos serão serializados configurando a propriedade `ConfigureJsonSerializerSettings`.  Não se engane, o mecanismo de serialização JSON usado pelo Orleans é o [Json.NET][json.net], então você pode ver com [mais detalhes como configurar um `JsonSerializerSettings`][json.net-serializersettings].

E reparou que não damos um nome ao tipo de conexão, neste caso `GrainTable` ? Isso acontece porque podemos ter mais de uma conexão nomeada, e inclusive cada conexão pode usar bases de dados diferentes (lembrando que podemos escolher entre **SQL Server**, **Oracle**, **MySQL/MariaDB** ou **Postegre**). Mas se usamos apenas uma conexão, podemos usar o método de extensão `AddAdoNetGrainStorageAsDefault`, que exige apenas as configurações de conexão.

```csharp
.AddAdoNetGrainStorageAsDefault(options =>
{
	options.Invariant = "System.Data.SqlClient";
	options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
	options.UseJsonFormat = true;
})
```

# Preparando o Grain para a persistência de Objetos

Antes de mais nada, não é o **Grain** que é persistido, são objetos que o **Grain** usa que podem ser serializados e desserializados. Em primeiro lugar, os objetos persistidos devem ser marcados com o atributo `Serializable`. Vale destacar que não é obrigatório que os objetos 'filhos' tenham o mesmo atributo. No exemplo abaixo, o objeto que realmente será serializado é `ConversationState`. 

```csharp
[Serializable]
public class ConversationState
{
	public List<ConversationPhraseState> Phrases { get; set; }
}

public class ConversationPhraseState
{
	public DateTime DateTime { get; set; }
	public string Phrase { get; set; }
}
```

No projetos dos **Grains**, o pacote `Microsoft.Orleans.Core.Abstractions` deve ser instalado e os objetos persistidos deve ser carregados através de um `IPersistentState<T>`. Neste exemplo, o **Grain** `ConversationGrain` deve ter um campo do tipo `IPersistentState<ConversationState>`:

```csharp
private readonly IPersistentState<ConversationState> _conversationState;
```

No construtor do **Grain**, fazemos o carregamento do objeto como se fosse uma injeção de dependência, mas o parâmetro deve ser marcado com o atributo `PersistentState` juntamente com o nome que identifique logicamente o objeto. Vale destacar que o atributo `PersistentState` aceita um segundo parâmetro opcional com o nome da conexão de dados que deve ser usada para persistir o objeto - se a conexão fosse nomeada, é aquele nome que deveria ser passado aqui.

Obviamente você pode ter **Grains** com mais de um objeto de persistência, mas cada objeto passado precisa do atributo `PersistentState`.

```csharp
public ConversationGrain([PersistentState("conversation")] IPersistentState<ConversationState> conversationState)
{
	_conversationState = conversationState;
}
```

# Lendo, gravando e apagando objetos persistidos

Cada variável do tipo `IPersistentState` apresenta três métodos que devem ser usados para ler, escrever ou apagar o respectivo objeto persistido:

- `ReadStateAsync`: recarrega o objeto persistido, todas as mudanças não salvas feitas são descartadas.

- `WriteStateAsync`: persiste o objeto na base de dados.
- `ClearStateAsync`: limpa o objeto para o estado padrão.

O objeto carregado estará disponível na propriedade `State` do ``IPersistentState`, mas vale destacar um aspecto do comportamento de persistência: Todos os objetos carregados através do construtor do  **Grain** são lidos da base de dados assim que o **Grain** é ativado. Isso significa que não precisamos fazer chamadas `ReadStateAsync`. Mas isso pode gerar um problema se o objeto em questão tem listas como propriedades, já que o carregamento inicial não é uma lista vazia, e sim uma variável `null`.

Neste exemplo, há uma sobrecarga do método `OnActivateAsync` para que a propriedade `Phrases` seja carregada corretamente. Note que se o parâmetro `Phrases` for `null` (e é `null` quando o **Grain** é ativado pela primeira vez), já o preenchemos com uma lista vazia e já o salvamos com o método `WriteStateAsync`.

```csharp
public override async Task OnActivateAsync()
{
    if (_conversationState.State.Phrases == null)
    {
        _conversationState.State.Phrases ??= new List<ConversationPhraseState>();
        await _conversationState.WriteStateAsync();
    }

    await base.OnActivateAsync();
}
```

O método `Say` do `ConversationGrain` apenas grava quando a mensagem foi recebida. Simplesmente um novo objeto do tipo `ConversationPhraseState` é adicionado à lista e o método `WriteStateAsync` é chamado para persistir o objeto inteiro.

```csharp
public async Task Say(string message)
{
    DeactivateOnIdle();
    _conversationState.State.Phrases.Add(new ConversationPhraseState { DateTime = DateTime.Now, Phrase = message});
    await _conversationState.WriteStateAsync();
}
```

Já no método `ShowHistory` usamos o método `ReadStateAsync`  para carregar as informações persistidas na memória (garantindo que temos as informações mais atualizadas neste instante) e retornamos o histórico de conversas, por assim dizer.

```csharp
public async Task<IEnumerable<string>> ShowHistory()
{
    DeactivateOnIdle();
    await _conversationState.ReadStateAsync();
    return _conversationState.State.Phrases
        .OrderBy(x => x.DateTime)
        .Select(x => $"{x.DateTime} - {x.Phrase}").ToList();
}
```

Finalmente, o método `EraseHistory` do **Grain** usa o método `ClearStateAsync`  para apagar todo o histórico - isso faz com que a propriedade `Phrases` volte a ser `null`!

```csharp
public async Task EraseHistory()
{
    DeactivateOnIdle();
    await _conversationState.ClearStateAsync();
}
```

# Exemplificando a persistência de objetos no Client

De forma prática, o **Client** apenas faz as chamadas para os métodos do **Grain**, ele não precisa saber nem se há alguma espécie de persistência do outro lado, isso é completamente transparente por parte do **Client**. Por exemplo, o trecho de código abaixo envia três mensagens e as recupera. O **Client** não sabe que o **Grain** está

```csharp
// first grain
var conversation = client.GetGrain<IConversationGrain>(0);
await conversation.Say("First message");
await conversation.Say("Second message");
await conversation.Say("Third message");
var resultConversation = (await conversation.ShowHistory()).ToList();
Console.WriteLine($"Conversation with Primary Key 0 has {resultConversation.Count} messages");
foreach (var s in resultConversation)
	Console.WriteLine(s);
```

Logo após, criamos um segundo **Grain** com outra chave primária e fazemos o mesmo procedimento, com outras mensagens. Isso mostra que não há conflito entre **Grains** porque a chave primária faz a segregação - mesmo que ela não seja usada nos objetos persistidos pelo **Grain**.

```csharp
// second grain
var otherConversation = client.GetGrain<IConversationGrain>(1);
await otherConversation.Say("Another message");
await otherConversation.Say("And another message");
var resultOtherConversation = (await otherConversation.ShowHistory()).ToList();
Console.WriteLine($"Conversation with Primary Key 1 has {resultOtherConversation.Count} messages");
foreach (var s in resultOtherConversation)
	Console.WriteLine(s);
```

Por último, nós limpamos o histórico de mensagens enviadas ao segundo **Grain**. Novamente, isso não afeta a persistência do primeiro **Grain**, pois são objetos diferentes.

```csharp
// second grain, erasing history
await otherConversation.EraseHistory();
resultOtherConversation = (await otherConversation.ShowHistory()).ToList();
Console.WriteLine($"Conversation with Primary Key 1 has {resultOtherConversation.Count} messages");
```

# Sumário

- Quando usamos a persistência do Orleans, estamos falando na serialização e desserialização de objetos!
- Podemos usar mais de uma base de dados para persistir diferentes objetos. Neste caso, cada base de dados de persistência precisa ter um nome interno específico para identificação.
- A serialização de objetos pode ser feita com XML, JSON ou binário (que é o padrão).
- Mais de um objeto pode ser persistido por **Grain**.
- A persistência é individualizada por chave primária do **Grain** - objetos de um **Grain** não afetam objetos de outro.
- Todos os objetos são carregados assim que o **Grain** é ativado, mas nada é salvo automaticamente, lembre-se disso!



[readme-parte2]: https://github.com/prrandrade/OrleansStudy/tree/master/Parte%202%20-%20Computa%C3%A7%C3%A3o%20distribu%C3%ADda%20e%20persist%C3%AAncia%20com%20o%20Orleans
[json.net]: https://www.newtonsoft.com/json
[json.net-serializersettings]: https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonSerializerSettings.htm
[05-BasicClusterAdoNet]: http://github.com/prrandrade/OrleansStudy/tree/master/Projetos/05-BasicClusterAdoNet

