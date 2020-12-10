# Arquitetura de sistemas com o Orleans

- [Introdução](#1-introdução)
- [Injeção de dependência nos Silos](#2-injeção-de-dependência-nos-silos)
- [Projeto SiloDependencyInjection](#3-projeto-silodependencyinjection)
- [Injeção de dependência no Client](#4-injeção-de-dependência-no-client)
- [Projeto ClientDependencyInjection](#5-projeto-clientdependencyinjection)

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