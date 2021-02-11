# Projeto StatelessGrain

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Diferenças de conceito no Stateless Grain](#3-diferenças-de-conceito-no-stateless-grain)
- [Chave primária de um Stateless Grain](#4-chave-primária-de-um-stateless-grain)
- [Sumário](#5-sumário)

# 1. Introdução

Aqui vamos ver o comportamento de um **Stateless Grain** e como ele pode ser usado para melhorar o desempenho de um sistema feito com Orleans em determinadas situações.

<div align="right">	
[Voltar](#projeto-statelessgrain)
</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker](https://www.docker.com). Use a linha de comando que eu separei no repositório [DockerShortcuts](https://github.com/prrandrade/DockerShortcuts).

<div align="right">	
[Voltar](#projeto-statelessgrain)
</div>

# 3. Diferenças de conceito no Stateless Grain

Existem duas diferenças básicas conceituais no **Stateless Grains**. A primeira é que um **Stateless Grain** sempre será executado **localmente** - isto é, no **mesmo Silo** o qual ele foi chamado. Isso pode aumentar o desempenho do sistema como um todo, já que é uma possível chamada remota a menos.

A segunda é que o **Stateless Grain** pode ser executado mais de uma vez com a mesma chave primária ao mesmo tempo. Os **Grains** normais sempre são seriais em relação à chave primária (esta é a graça do Virtual Actor, afinal). Os **Stateless Grains** podem ser executados paralelamente com a mesma chave primária em diferentes **Silos** e até mesmo no MESMO **Silo** - ou seja, os **Silos** não guardam o individualizam o estado de ativação dos **Stateless Grains**, daí vem seu nome, _stateless_ (sem estado).

Em termos de desenvolvimento, não há praticamente nenhuma diferença prática entre um **Grain** normal e um **Stateless Grain**. Você faz a interface, define qual o tipo de chave primária do **Grain** e faz sua implementação. Na implementação, simplesmente coloque o atributo `StatelessWorker` na classe do **Grain** e pronto, seu **Grain** já é do tipo **Stateless**! A quantidade de **Stateles Grains** com a mesma chave primária que podem ser executadas simultaneamente no mesmo **Silo** depende da configuração do atributo. O caso abaixo, por exemplo, permite que apenas um **Stateless Grain** seja ativado por **Silo**. Se nenhuma configuração de limitação for feita, então a quantidade máxima de **Stateless Grains** ativadas por **Silo** é a quantidade de processadores lógicos na máquina onde o **Silo** é executado.

```csharp
public interface IExampleGrain : IGrainWithIntegerKey
{
	Task Do();
}

[StatelessWorker(1)]
public class ExampleGrain : Grain, IExampleGrain
{
	public async Task Do()
	{
		Console.WriteLine($"{DateTime.Now:HH:mm:ss.ffff} - {this.GetPrimaryKeyLong()} - Done!");
		await Task.Delay(200);
	}
}
```

<div align="right">	
[Voltar](#projeto-statelessgrain)
</div>

# 4. Chave primária de um Stateless Grain

Os limites de ativação de **Stateless Grains** são por **Silo** e por chave primária. Isso significa que você pode ultrapassar o limite imposto no atributo `StatelessWorker` se usar diferentes chaves primárias na hora de ativar o **Stateless Grain**. Justamente por isso, normalmente os **Statemess Grains** usam `int` como chave primária e são chamados sempre com a chave primária **0**. Lembre-se também que, normalmente, **Stateless Grains** são usados para tarefas internas dos **Silos** (como telemetria, por exemplo) e não costumam ser acessados pelos **Clients**.

# 5. Sumário

- **Stateless Grains** são **Grains** que podem ser ativados ao mesmo tempo em diferentes **Silos**; normalmente são usados para tarefas internas.
- Ues sempre a mesma chave primária na hora de ativar o **Stateless Grain**, para que as configurações de limite de ativação sejam obedecidas, já que estas configurações são por chave primária.
- Se nenhuma configuração de limite for marcada no atributo `StatelessWorker`, o limite de ativações simultâneas será a quantidade de procesadores na máquina.


