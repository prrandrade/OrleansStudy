# Arquitetura de sistemas com o Orleans

- [Introdução](#1-introdução)
- [Injeção de dependência nos Silos](#2-injeção-de-dependência-nos-silos)
- [Projeto SiloDependencyInjection](#3-projeto-silodependencyinjection)
- [Injeção de dependência no Client](#4-injeção-de-dependência-no-client)
- [Projeto ClientDependencyInjection](#5-projeto-clientdependencyinjection)
- [Grains chamando outros Grains](#6-grains-chamando-outros-grains)
- [Projeto GrainsCallingGrains](#7-projeto-grainscallinggrains)
- [Quebrando o conceito de Virtual Actor dos Grains](#8-quebrando-o-conceito-de-virtual-actor-dos-grains)
- [Projeto GrainReentrancy](#9-projeto-grainReentrancy)
- [Quebrando o conceito de Virtual Actor dos Grains em métodos específicos](#10-quebrando-o-conceito-de-virtual-actor-dos-grains-em-métodos-específicos)
- [Projeto GrainMethodsInterleaving](#11-projeto-grainmethodsinterleaving)
- [Sistemas com o Orleans e sem Clients](#12-sistemas-com-o-orleans-e-sem-clients)
- [Projeto GrainService](#13-projeto-grainservice)
- [Stateless Grains](#14-stateless-grains)
- [Projeto StatelessGrain](#15-projeto-statelessgrain)

# 1. Introdução

Já cobrimos aspectos básicos e alguns aspectos mais avançados do Orleans, conseguindo, com isso, já começar a montar sistemas completos distribuídos. Agora vamos ver como conceitos conhecidos de arquitetura e padrões de projeto se encaixam no Orleans.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 2. Injeção de dependência nos Silos

Penso que o primeiro aspecto a se pensar em arquitetura de sistemas com o Orleans é a boa e velha injeção de dependência - ainda mais que o .NET Core e o atual .NET 5.0 apresentam um mecanismo interno de injeções de dependência, sem precisar de pacotes de terceiros, como [Ninject](http://www.ninject.org/), [Simple Injector](https://simpleinjector.org/) e outros.

Pode-se argumentar que o mecanismo padrão de injeção de dependência do .NET não é tão flexível quanto alguns pacotes externos - o que é verdade. Mas tantos cenários podem ser cobertos com injeções básicas de dependência que, na prática, as limitações não atrapalham. E os **Silos** já apresentam este mecanismo.

Veja, quando o **Client** acessa o **Cluster** para executar os **Grains**, a execução de fato é em algum **Silo**. Portanto, é o **Silo** que faz todo o processamento local, incluindo o carregamento dos **Grains**. Portanto, faz sentido que ele seja o responsável por injetar as dependências de cada **Grain**.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 3. Projeto SiloDependencyInjection

No [projeto SiloDependencyInjection](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/11-SiloDependencyInjection), vamos ver na prática como configurar a injeção de dependência nos **Silos** para que o **Grains** recebam outros objetos (provavelmente contendo regras de negócio e acesso a dados) quando são usados.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 4. Injeção de dependência no Client

O raciocínio da injeção de dependência nos **Silos** é exatamente o contrário nos **Clients**. Veja, um **Silo** é o motor da aplicação que roda no lado do servidor, o **Client** é apenas um recurso que roda na aplicação do lado do cliente. Portanto, nós não fazemos injeções de dependência no **Client**, nós injetamos o **Client** como dependência de outros objetos.

E isso também faz sentido ao percebermos que o **Client** (e os **Silos**) não apresentam configurações de autenticação e autorização, por exemplo. Isso não é problema do Orleans, isso é problema do **Client**. Ou para ser mais preciso, problema de quem usa o **Client.** como uma dependência - uma WebApi por exemplo.


<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 5. Projeto ClientDependencyInjection

No [projeto ClientDependencyInjection](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/12-ClientDependencyInjection), criamos uma WebApi que uma o **Client** do Orleans como uma dependência que pode ser usada durante as chamadas. Note que lógicas de autenticação e autorização não ficariam a cargo do **Client** do Orleans, ficariam a cargo da WebApi.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 6. Grains chamando outros Grains

Todos os exemplos usados até o presente momento partem do mesmíssimo princípio: o que de o **Client** precisa de um método e apenas um método do **Grain** e que este método é público. Mas e quando a lógica de negócio está espalhada em vários **Grains** (como aliás, deveria estar)? Podemos chamar assicronamente 'vários **Grains**, claro, mas se se precisamos chamá-los numa ordem específica? Ou quando não queremos que o cliente saiba desta ordem? A solução é fazer com que os **Grains** usem uns aos outros - o que totalmente possível!

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 7. Projeto GrainsCallingGrains

No [projeto GrainsCallingGrains](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/13-GrainsCallingGrains), vamos ver como os **Grains** podem chamar uns aos outros para distribuir ainda mais o processamento dos métodos - e sem o **Client** ficar sabendo disso!

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 8. Quebrando o conceito de Virtual Actor dos Grains

Já falamos anteriormente que uma das partes mais difíceis de sistemas distribuídos é fazer com que as requisições sejam distribuídas de forma serializada, fazendo com que certas chamadas sejam realizadas em ordem de chegada. E isso está embutido no Orleans, com o conceito de virtual actor, controlado na prática com as chaves primárias dos Grains. Mas e quando esta organização do Orleans acaba atrapalhando a lógica de negócio? É possível 'passar por cima' de um Virtual Actor? Sim, e de diferentes maneiras!

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 9. Projeto GrainReentrancy

No [projeto GrainReentrancy](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/14-GrainReentrancy) podemos ver como configurar **Grains** para que o início da execução de um método do mesmo **Grain** (mesma chave primária) não dependa do final da execução de outro método do mesmo **Grain**.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 10. Quebrando o conceito de Virtual Actor dos Grains em métodos específicos

O problema de se usar o atributo `GrainReentrancy` no **Grain** é que todos os métodos deixam de usar as vantagens do modelo de virtual Actor do Orleans, o que não necessatiamente queremos sempre. Se o problema está em métodos específicos, a resposta é outro atibuto, que vamos conhecer a seguir.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 11. Projeto GrainMethodsInterleaving

No [projeto GrainMethodsInterleaving](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/15-GrainMethodsInterleaving), vemos como quebrar o conceito de virtual actor do Orleans apenas em métodos específicos de um **Grain**, mantendo todas as vantagens desse sistema no resto do **Grain**.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 12. Sistemas com o Orleans e sem Clients

Em absolutamente todos os exemplos que vimos e fizemos até agora, a parte servidor do Orleans - ou seja, **Grains** e **Silos** precisaram ser provocados pelos **Clients** para começar o processamento. Só que existe um tipo especial de **Grain** que é iniciado com o **Silo**, aceita outros serviços via injeção de dependência e consegue lidar com os **Grains** normais - o **GrainService**.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 13. Projeto GrainService

No [projeto GrainService](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/16-GrainService), temos um exemplo claro de sistema com **Silo**, **Grain** e **GrainService** que não precisa de **Client** (ou seja, não precisa de estímulo externo) para começar um processamento de exemplo.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 14. Stateless Grains

Já sabemos que apenas uma ativação de um **Grain** pode ser feita usando a mesma chave primária, em todos os **Silos** (este é um dos motivos pelos quais eles se comunicam entre si). Mas existe um tipo especial de **Grain** o qual pode ser ativado mais de uma vez em diferentes **Silos** - e mais de uma vez no mesmo **Silo**. Estes são os **Stateless Grains** (sem estado porque não há o controle remoto de estado do **Grain** entre **Silos**). O engraçado é que a programação em si de um **Stateless Grain** é exatamente igual a de um **Grain** normal, com a exceção de um atributo. É isso que veremos a seguir.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 15. Projeto StatelessGrain

No [projeto StatelessGrain](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/17-StatelessGrain) vamos ver o comportamento de um **Stateless Grain** e como suas características únicas podem ser usadas para otimizar certos processos distribuídos.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>