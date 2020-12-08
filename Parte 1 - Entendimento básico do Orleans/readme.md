# Entendimento básico do Orleans

- [Introdução](#1-introdução)
- [Nomenclatura](#2-nomenclatura)
- [Projeto HelloWorld](#3-projeto-helloworld)
- [Após o projeto HelloWorld](#4-após-o-projeto-helloworld)
- [Projeto PrimaryKeys](#5-projeto-primarykeys)
- [Após o projeto PrimaryKeys](#6-após-o-projeto-primarykeys)
- [Projeto RetrievingPrimaryKeys](#7-projeto-retrievingprimarykeys)
- [Após o projeto RetrievingPrimaryKeys](#8-após-o-projeto-retrievingprimarykeys)
- [Projeto GrainActivation](#9-projeto-grainactivation)
- [Após o projeto GrainActivation](#10-após-o-projeto-grainactivation)
- [Sumário dos projetos](#11-sumário-dos-projetos)
- [Conclusão](#12-conclusão)

# 1. Introdução

Direto ao ponto, o [Microsoft Orleans][orleans] é um projeto que permite criar e executar sistemas distribuídos de forma simples, abstraindo os conceitos de distribuição de tarefas, quem executa o que, e como um processamento é retomado caso a máquina que esteja o fazendo saia do ar. Mas primeiro, vamos entender a nomenclatura básica do que significa cada coisa do Orleans (spoiler: pense basicamente numa arquitetura cliente-servidor, mas turbinada).

# 2. Nomenclatura

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

Parece meio óbvio, mas vale a pena destacar. Os clientes que acessam os **Silos** para o processamento dos **Grains** são chamados de **Silo Clients**, ou apenas **Clients**. Insisto: os **Clients** não executam o código dos **Grains**, são os **Silos** que fazem o trabalho sujo.

# 3. Projeto HelloWorld

Obviamente vamos começar... ora, do começo! O [projeto HelloWorld][01-HelloWorld] mostra um exemplo BEM SIMPLES de como **Client**, **Silo**, **Grains**  e **Interfaces** funcionam.

# 4. Após o Projeto HelloWorld

OK, se você passou pelo Hello World, já viu como um **Client** se conecta ao **Silo** para executar o código de um **Grain**. Mas deve ter achado estranho o fato de que precisamos de uma chave primária para utilizar um **Grain**. Isso é necessário porque o Microsoft Orleans apresenta o conceito de Virtual Actor. E para entender o conceito de Virtual Actor, vamos entender o conceito de Actor.

### Actor

A Wikipedia já tem uma [explicação BASTANTE detalhada sobre o Actor][actor_concept] mas explicando de forma bem simplificada, um actor é a menor unidade de programação na computação distribuída. Ao receber mensagens, um actor pode enviar mensagens para outros actors, criar novos actors e definir o comportamento que será usado ao receber outras mensagens. Só que ao usar actors, vários problemas inerentes da computação distribuída surgem:

- Precisamos garantir que as mensagens sejam processadas APENAS uma vez, mesmo que o sistema esteja espalhado em várias máquinas.
- Precisamos garantir que as mensagens sejam processadas mesmo a máquina que o está processando não esteja mais disponível.
- Precisamos garantir que as informações processadas num actor estejam disponíveis em todos os nós no sistema distribuído.
- Precisamos garantir que, se uma máquina de um sistema distribuído não estiver disponível, todo o sistema continue de pé, de preferência sem intervenção manual.

### Virtual Actor

O Orleans, através dos **Grains** abstraí toda esta parte burocrática dos Actors - usando o conceito de **Virtual Actor**. Foi exatamente o que [fizemos no HelloWorld][01-HelloWorld] ao fazer o **Client** ativar um **Grain** que é executado no **Silo**. E na ativação, passamos uma chave primária para garantir que a execução é única do lado do servidor. Vamos ver isso com calma no próximo exemplo.

# 5. Projeto PrimaryKeys

Uma das graças do Virtual Actor é que a gente não precisa se preocupar com a questão da concorrência dos métodos no mesmo **Grain**. [O projeto PrimaryKeys][02-PrimaryKeys] demonstra que, a partir do momento que a chave primária é a mesma, a execução dos métodos é literalmente serial, apenas quando um método é executado que outro método é executado.

# 6. Após o projeto PrimaryKeys

Já sabemos que não precisamos nos preocupar com a concorrência de **Grains**, se eles forem ativados com a mesma chave primária - o que é ótimo para serializar operações do mesmo usuário, por exemplo. Além disso, nada impede que a chave primária também seja a chave primária de uma base de dados- representando um usuário ou uma operação. Mas como usar a chave primária durante a lógica de negócio? Fácil, resgatando os valores dentro do **Grain**.

# 7. Projeto RetrievingPrimaryKeys

[No projeto RetrievingPrimaryKeys][03-RetrievingPrimaryKeys], vamos ver como podemos recuperar chaves primárias de **Grains**, e conhecer mais a fundo os cinco diferentes tipos de chaves primárias que podem ser usadas para individualizar **Grains** (já pincelamos sobre isso no [HelloWorld][01-HelloWorld]).

# 8. Após o projeto RetrievingPrimaryKeys

Isso já foi falado algumas vezes, mas vale a pela relembrar: a lógica dos **Grains** é executada no servidor - nos **Silos**, e não no **Client**. Isso significa que, de alguam forma, os **Grains** precisam começar a existir do lado do servidor para que seus métodos sejam chamados pelo **Client**. Este processo dentro do Orleans é chamado de ativação, e pode acontecer quando o Grain é carregado juntamente com sua chave primária.

E note que eu disse **pode** acontecer, porque o Orleans tem uma certa inteligência para manter **Grains** carregados e evitar processamento de ativações e desativações que não fazem sentido. 

# 9. Projeto GrainActivation

Através do [projeto GrainActivation][04-GrainActivation], vamos aprender como usar a ativação e desativação dos **Grains** juntamente com lógica de negócio customizada.

# 10. Após o projeto GrainActivation

Conseguimos sem muito mistério adicionar lógica de negócio nos métodos básicos do ciclo de vida de um **Grain** - o que já cobre vários cenários diferentes.

# 11. Sumário dos projetos

- Interfaces de **Grains** são conhecidas por todos os projetos, e devem implementar uma das interfaces que garante uma chave primaria.

- Métodos dos **Grains** declarados nas intercaces devem sempre devolver uma `Task` ou uma `ValueTask`.

- Implementações dos **Grains**  devem, além de implementar as interfaces, herdar da classe **Grain**
- **Silos** conhecem as interfaces e as implementações dos **Grains**, pois os **Grains** são executados nos **Silos**.

- **Clients** só conhecem as interfaces dos **Grains**, estes se conectam nos **Silos** e recebem o retorno dos métodos.

- Precisamos de uma chave primária para ativar um **Grain** no **Client**.

- Não importa de onde os **Grains** são ativados, se a chave primária é a mesma, para fins práticos, o **Grain** é o mesmo.

- Métodos do mesmo **Grain** (mesma chave primária) são por definição seriais, precisamos esperar o retorno de um método para conseguir executar outros métodos.

- Métodos de **Grains** com chaves primárias diferentes podem obviamente ser chamados de forma paralela.

- Chaves primárias podem (e devem) ser usadas para individualizar a regra de negócio, elas não precisam ser usadas apenas para a ativação dos **Grains**.

- Existem cinco tipos diferentes de chaves primárias no Orleans para individualizar **Grains**.

- No caso de chaves compostas, não é possível nem ativar **Grains** sem ambos os componentes da chave e nem recuperar apenas um componente da chave primária (o que conseguimos fazer é ignorar o segundo componente de uma chave composta graças ao C# 7.0).

- Podemos adicionar lógica de negócio customizada na ativação e na desativação dos **Grains**.

- Podemos pedir a desativação imediata de **Grains** caso seja necessário.

# 12. Conclusão

Após passar por todos os exemplos, conseguimos cobrir os aspectos mais básicos do Orleans:

- Sabemos como é a lógica básica do Orleans, onde **Clients** se conectam num **Cluster**, formado por um conjunto de **Silos**. Estes **Silos** executam o código do **Grain** e devolvem os resultados dos métodos para os **Clients**. Ou seja, a lógica de negócio **é executada do lado do servidor**!

- **Grains** são identificados através de chaves primárias, que podem ser usadas na lógica de negócio. Estas chaves primárias, como o nome já indica, individualizam e serializam a execução do código.

- A ativação e desativação dos **Grains** são os passos mais fundamentais no ciclo de vida, e lógica de negócio customizada pode ser inserida nestes passos - inclusive a desativação do **Grain** pode ser chamada via método próprio.

- Se já houver uma referência de um **Grain**, não precisamos em precisar ativá-lo com a chave primária novamente. Uma chamada de qualquer método do **Grain** já o ativa (e executa a lógica de negócio possivelmente atrelada).

Com isso, já conseguimos montar uma estrutura local com o Orleans, embora ele ainda não se justifique como tecnologia apenas com estes elementos - não estamos fazendo nada muito diferente de uma API, sinceramente. É a partir de agora que vamos ver onde e como o Orleans realmente se destaca.

[actor_concept]: https://en.wikipedia.org/wiki/Actor_model#Fundamental_concepts
[orleans]: https://github.com/dotnet/orleans
[01-HelloWorld]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/01-HelloWorld
[02-PrimaryKeys]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/02-PrimaryKeys
[03-RetrievingPrimaryKeys]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/03-RetrievingPrimaryKeys
[04-GrainActivation]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/04-GrainActivation