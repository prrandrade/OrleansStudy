# Microsoft Orleans na prática

- [Introdução](#introducao)
- [Nonemclatura](#nomenclatura)
- [Hello World](#hello-world)

# Introdução

Direto ao ponto, o [Microsoft Orleans](https://github.com/dotnet/orleans) é um projeto que permite criar e executar sistemas distribuídos de forma simples, abstraindo os conceitos de distribuição de tarefas, quem executa o que, e como um processamento é retomado caso a máquina que esteja o fazendo saia do ar. Mas primeiro, vamos entender a nomenclatura básica do que significa cada coisa do Orleans (spoiler: pense basicamente numa arquitetura cliente-servidor, mas turbinada).

# Nomemclatura

### Grains

A unidade básica de processamento do Orleans é o **Grain**, basicamente uma classe com regras de negócio. A diferença é que o cliente chama esta classe, mas a execução da mesma é feita no servidor (ou servidores, já que estamos falando de sistemas distribuídos). O cliente não sabe a lógica por trás de um **Grain** - aliás, nem os **Grains** devem saber a lógica uns dos outros.

Isso significa que além do projeto com as classes qhe herdam de **Grain**, você precisará ter um projeto de interfaces, que será usado pelos clientes e pelos servidores. Na hora de referenciar um **Grain**, nós usamos as interfaces, e não as implementações.

Vale destacar que, **todos** os métodos de um **Grain** devem sempre retornar uma `Task`, uma `Task<T>` ou uma `ValueTask<T>`. O **Grain** é pensado para ser executado num contexto multithread.

### Silos

Já sabemos que os **Grains** não rodam do lado do cliente, rodam do lado do servidor... pois bem, cada serviço que roda do lado do servidor dentro da arquitetura do Orleans é um **Silo**, simples assim! Veja, apenas os **Silos** conhecem as implementações dos **Grains**, a lógica de negócio dos **Grains** é executada nos **Silos**.

### Cluster

Então, se temos vários **Grains** sendo executados num único **Silo**, isso já é o Orleans em ação, se dúvida. Mas a graça do Orleans é justamente distribuir este processamento em vários pontos - neste caso, em vários **Silos**. O conjunto de **Silos** que conseguem processar os mesmos **Grains** (não vou me cansar de lembrar que o processamento do **Grain** é do lado do servidor!) é chamado de **Cluster**.

A graça de um **Cluster** com vários **Silos** é que estes se comunicam entre si para coordenar o processamento dos **Grains**. Assim, em teoria, você não terá um **Silo** sobrecarregado ou algum processamento de um **Grain** feio pela metade porque o **Silo** que o estava processando caiu.

### Silo Clients

Parece meio obvio, mas vale a pena destacar. Os clientes que acessam os **Silos** para o processamento dos **Grains** são chamados de **Silo Clients**, ou apenas **Clients**. Insisto: os **Clients** não executam o código dos **Grains**, são os **Silos** que fazem o trabalho sujo.

# Hello World

Obviamente vamos começar... ora, do começo! Dentro da pasta study/01-HelloWorld temos um exemplo BEM SIMPLES de como **Client**, **Silo**, **Grains**  e **Interfaces** funcionam.
