# Microsoft Orleans na prática

- [Introdução](#introdução)
- [Nomenclatura](#nomenclatura)
- [Projeto HelloWorld](#projeto-helloworld)
- [Após o projeto HelloWorld](#após-o-projeto-helloworld)
- [Projeto PrimaryKeys](#projeto-primarykeys)
- [Após o projeto PrimaryKeys](#após-o-projeto-primarykeys)

# Introdução

Direto ao ponto, o [Microsoft Orleans](https://github.com/dotnet/orleans) é um projeto que permite criar e executar sistemas distribuídos de forma simples, abstraindo os conceitos de distribuição de tarefas, quem executa o que, e como um processamento é retomado caso a máquina que esteja o fazendo saia do ar. Mas primeiro, vamos entender a nomenclatura básica do que significa cada coisa do Orleans (spoiler: pense basicamente numa arquitetura cliente-servidor, mas turbinada).

# Nomenclatura

### Grains

A unidade básica de processamento do Orleans é o **Grain**, basicamente uma classe com regras de negócio. A diferença é que o cliente chama esta classe, mas a execução da mesma é feita no servidor (ou servidores, já que estamos falando de sistemas distribuídos). O cliente não sabe a lógica por trás de um **Grain** - aliás, nem os **Grains** devem saber a lógica uns dos outros.

Isso significa que além do projeto com as classes que herdam de **Grain**, você precisará ter um projeto de interfaces, que será usado pelos clientes e pelos servidores. Na hora de referenciar um **Grain**, nós usamos as interfaces, e não as implementações.

Vale destacar que, **todos** os métodos de um **Grain** devem sempre retornar uma `Task`, uma `Task<T>` ou uma `ValueTask<T>`. O **Grain** é pensado para ser executado num contexto multithread.

### Silos

Já sabemos que os **Grains** não rodam do lado do cliente, rodam do lado do servidor... pois bem, cada serviço que roda do lado do servidor dentro da arquitetura do Orleans é um **Silo**, simples assim! Veja, apenas os **Silos** conhecem as implementações dos **Grains**, a lógica de negócio dos **Grains** é executada nos **Silos**.

### Cluster

Então, se temos vários **Grains** sendo executados num único **Silo**, isso já é o Orleans em ação, se dúvida. Mas a graça do Orleans é justamente distribuir este processamento em vários pontos - neste caso, em vários **Silos**. O conjunto de **Silos** que conseguem processar os mesmos **Grains** (não vou me cansar de lembrar que o processamento do **Grain** é do lado do servidor!) é chamado de **Cluster**.

A graça de um **Cluster** com vários **Silos** é que estes se comunicam entre si para coordenar o processamento dos **Grains**. Assim, em teoria, você não terá um **Silo** sobrecarregado ou algum processamento de um **Grain** feio pela metade porque o **Silo** que o estava processando caiu.

### Silo Clients

Parece meio obvio, mas vale a pena destacar. Os clientes que acessam os **Silos** para o processamento dos **Grains** são chamados de **Silo Clients**, ou apenas **Clients**. Insisto: os **Clients** não executam o código dos **Grains**, são os **Silos** que fazem o trabalho sujo.

# Projeto HelloWorld

Obviamente vamos começar... ora, do começo! [Dentro da pasta study/01-HelloWorld](https://github.com/prrandrade/OrleansStudy/tree/master/study/01-HelloWorld) temos um exemplo BEM SIMPLES de como **Client**, **Silo**, **Grains**  e **Interfaces** funcionam.

# Após o Projeto HelloWorld

OK, se você passou pelo Hello World, já viu como um **Client** se conecta ao **Silo** para executar o código de um **Grain**. Mas deve ter achado estranho o fato de que precisamos de uma chave primária para utilizar um **Grain**. Isso é necessário porque o Microsoft Orleans apresenta o conceito de Virtual Actor. E para entender o conceito de Virtual Actor, vamos entender o conceito de Actor.

### Actor

A Wikipedia já tem uma [explicação BASTANTE detalhada sobre o Actor](https://en.wikipedia.org/wiki/Actor_model#Fundamental_concepts) mas explicando de forma bem simplificada, um actor é a menor unidade de programação na computação distribuída. Ao receber mensagens, um actor pode enviar mensagens para outros actors, criar novos actors e definir o comportamento que será usado ao receber outras mensagens. Só que ao usar actors, vários problemas inerentes da computação distribuída surgem:

- Precisamos garantir que as mensagens sejam processadas APENAS uma vez, mesmo que o sistema esteja espalhado em várias máquinas.
- Precisamos garantir que as mensagens sejam processadas mesmo a máquina que o está processando não esteja mais disponível.
- Precisamos garantir que as informações processadas num actor estejam disponíveis em todos os nós no sistema distribuído.
- Precisamos garantir que, se uma máquina de um sistema distribuído não estiver disponível, todo o sistema continue de pé, de preferência sem intervenção manual.

### Virtual Actor

O Orleans, através dos **Grains** abstraí toda esta parte burocrática dos Actors - usando o conceito de **Virtual Actor**. Foi exatamente o que [fizemos no HelloWorld](https://github.com/prrandrade/OrleansStudy/tree/master/study/01-HelloWorld) ao fazer o Client ativar um **Grain** que é executado no **Silo**. E na ativação, passamos uma chave primária para garantir que a execução é única do lado do servidor. Vamos ver isso com calma no próximo exemplo.

# Projeto PrimaryKeys

Uma das graças do Virtual Actor é que a gente não precisa se preocupar com a questão da concorrência dos métodos no mesmo **Grain**. [Dentro da pasta study/02-PrimaryKeys](https://github.com/prrandrade/OrleansStudy/tree/master/study/02-PrimaryKeys), a partir do momento que a chave primária é a mesma, a executação dos métodos é literalmente serial, apenas quando um método é executado que outro método é executado.

# Após o projeto PrimaryKeys

Já sabemos que não nios precisamos nos preocupar com a concorrência de **Grains**, se eles forem ativados com a mesma chave primária - o que é otimo para serializar operações do mesmo usuário, por exemplo. Mas como usar a chave primária durante a lógica de negócio? Fácil, resgatando os valores durante os métodos.