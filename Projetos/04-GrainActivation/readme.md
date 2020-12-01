# Projeto GrainActivation

- [Introdução](#introdução)
- [Usando ativação e desativação ](#usando-ativação-e-desativação)
- [Explicando o uso dos métodos](#explicando-o-uso-dos-métodos)
- [Exemplo na prática](#exemplo-na-prática)
- [Curiosidades da desativação na documentação oficial](#curiosidades-da-desativação-na-documentação-oficial)
- [Sumário](#sumário)

# Introdução

Sem precisar se adentrar muito ao ciclo de vida dos **Grains** no Orleans, os dois estágios mais comuns durante a vida de um **Grain** são a ativação e a desativação.

### Ativação

O **Grain** é ativado no **Client** quando qualquer método é chamado - o que significa que, o **Client** não precisa se preocupar em ativar um **Grain** se já tiver alguma referência dele numa variável. A ativação **não** é feita quando o referência do **Grain** é criada (usando a chave primária).

### Desativação

A desativação do **Grain**, por definição, não é controlada pelo **Client**. Os Silos tem controle de quando eles percebem que nenhum **Client** precisa mais do **Grain** para removê-lo da memória.

# Usando ativação e desativação

Por definição, nem precisamos nos preocupar com ativação e desativação de **Grains**. Mas podemos usar estes dois passos do ciclo de vida do **Grain** para atrelar lógicas de negócio específicas. Basta sobrecarregar os métodos `OnActivateAsync` e `OnDeactivateAsync`, respectivamente. Isso também não significa que não possamos desativar um **Grain** quando quisermos, isso é atingível através do método `DeactivateOnIdle`.

# Explicando o uso dos métodos

Como exemplo, temos a classe `ExampleGrain`. Esta classe faz a sobrecarga do método `OnActivateAsync`, neste caso apenas escrevendo um log. Esta sobrecarga é útil para inicializar objetos quando o **Grain** precisa ser usado por algum **Client** ou outro **Grain**.

```
public override Task OnActivateAsync()
{
	_logger.LogInformation($"Grain with primary key {this.GetPrimaryKeyLong()} is activated.");
	return base.OnActivateAsync();
}
```

Já a sobrecarga do método `OnDeactivateAsync` pode ser usado para adicionar lógica de negócio de persistência de dados, por exemplo. Lembrando que normalmente o **Client** não dispara este método quando lida com o **Grain**.

```
public override Task OnDeactivateAsync()
{
	_logger.LogInformation($"Grain with primary key {this.GetPrimaryKeyLong()} is deactivated.");
	return base.OnDeactivateAsync();
}
```

Mas se você usar o método `DeactivateOnIdle` dentro de um método exposto pelo **Grain**, a desativação do mesmo será realizada assim que o método for executado (ou assim que possível).

```
public Task ExampleMethod(bool withDeactivation)
{
	if (withDeactivation)
		DeactivateOnIdle();
	return Task.CompletedTask;
}1
```

# Exemplo na prática

Vale a pena explicar o comportamento do **Client** e do **Grain** neste caso, pois estamos lidando com a ativação e desativação de **Grains**. A linha abaixo cria a referência do **Grain** e chama o método dele, o que o ativa e executa o método `OnActivateAsync`, como esperado.

```
var grain = client.GetGrain<IExampleGrain>(0);
await grain.ExampleMethod(false);
// log "Grain with primary key 0 is activated." será exibido
```

Agora quando chamamos o método `ExampleMethod(true)`, estamos chamando o método `DeactivateOnIdle` - o que força a desativação do **Grain** e, por conseqüência, a execução do método `OnDeactivateAsync`.

```
await grain.ExampleMethod(true);
// log "Grain with primary key 0 is deactivated." será exibido
```

A parte interessante vem agora. Se chamarmos o mesmo método `ExampleMethod(true)`, o **Grain** será ativado (porque havia sido desativado anteriormente) e desativado, pois o método `DeactivateOnIdle` também será chamado.

```
await grain.ExampleMethod(true);
// log "Grain with primary key 0 is activated." será exibido
// log "Grain with primary key 0 is deactivated." será exibido
```

# Curiosidades da desativação na documentação oficial

Vale a pena ver a [documentação oficial do Microsoft Orleans](https://dotnet.github.io/orleans/docs/host/configuration_guide/activation_garbage_collection.html) sobre a desativação automática de **Grains** que não estão mais sendo usados. O destaque é que o tempo padrão de inatividade para que um **Grain** seja de fato desativado é de 2 horas.

# Sumário

- Podemos adicionar lógica de negócio customizada na ativação e na desativação dos **Grains**.

- Podemos pedir a desativação imediata de **Grains** caso seja necessário.