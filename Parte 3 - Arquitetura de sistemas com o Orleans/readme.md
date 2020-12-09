# Arquitetura de sistemas com o Orleans

- [Introdução](#1-introdução)
- [Injeção de dependência nos Silos](#2-injeção-de-dependência-nos-silos)
- [Projeto SiloDependencyInjection](#3-projeto-silodependencyinjection)

# 1. Introdução

Já conbrimos aspectos básicos e alguns aspectos mais avançados do Orleans, conseguindo, com isso, já começar a montar sistemas completos distribuídos. Agora vamos ver como conceitos conhecidos de arquitetura e padrões de projeto se encaixam no Orleans.

<div align="right">
	
[Voltar](#arquitetura-de-sistemas-com-o-orleans)

</div>

# 2. Injeção de dependência nos Silos

Penso que o primeiro aspecto a se pensar em arquitetura de sistemas com o Orleans é a boa e velha injeção de dependência - ainda mais que o .NET Core e o atual .NET 5.0 apresentam um mecanismo interno de injeções de dependência, sem precisar de pacotes de terceiros, como [Ninject](http://www.ninject.org/), [Simple Injector](https://simpleinjector.org/) e outros.

Pode-se argumentar que o mecanismo padrão de injeção de dependência do .NET não é tão flexível quanto alguns pacotes externos - o que é verdade. Mas tantos cenários podem ser cobertos com injeções básicas de dependência que, na prática, as limitações não atrapalham. E os **Silos** já apresentam este mecanismo.

Veja, quando o **Client** acessa o **Cluster** para executar os **Grains**, a execução de fato é em algum **Silo**. Portanto, é o **Silo** que faz todo o processamento local, incluindo o carregamento dos **Grains**. Portanto, faz sentido que ele seja o responsável por injetar as dependências de cada **Grain**.

# 3. Projeto SiloDependencyInjection

No [projeto SiloDependencyInjection](https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/11-SiloDependencyInjection) Vamos ver na prática como configurar a injeção de dependência nos **Silos** para que o **Grains** recebeam outros objetos (provavelmente contendo regras de negócio e acesso a dados) quando são usados.



