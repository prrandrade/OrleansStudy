# Apêndice A - Code Snippets

### [Introdução](#1-introdução)

### [Básico](#2-básico)

- [Pacotes necessários para o projeto de interfaces de Grains](#pacotes-necessários-para-o-projeto-de-interfaces-de-grains)
- [Pacotes necessários para o projeto de implementações de Grains](#pacotes-necessários-para-o-projeto-de-implementações-de-grains)
- [Pacotes necessários para o projeto do Silo](#pacotes-necessários-para-o-projeto-do-silo)
- [Pacotes necessários para o projeto do Client](#pacotes-necessários-para-o-projeto-do-client)
- [Bootstrap do Silo em ambiente local](#bootstrap-do-silo-em-ambiente-local)
- [Bootstrap do Client em ambiente local](#bootstrap-do-client-em-ambiente-local)

### [Básico com Logging](#3-básico-com-logging)

- [Pacotes necessários para o projeto do Silo com logging no console](#pacotes-necessários-para-o-projeto-do-silo-com-logging-no-console)
- [Pacotes necessários para o projeto do Client com logging no console](#pacotes-necessários-para-o-projeto-do-client-com-logging-no-console)
- [Bootstrap do Silo em ambiente local com logging no console](#bootstrap-do-silo-em-ambiente-local-com-logging-no-console)
- [Bootstrap do Client em ambiente local com logging no console](#bootstrap-do-client-em-ambiente-local-com-logging-no-console)

### [Configuração básica dos Grains](#4-configuração-básica-dos-grains)

- [Implementando e recuperando chaves primárias dos Grains](#implementando-e-recuperando-chaves-primárias-dos-grains)
- [Sobrecarga na ativação e desativação dos Grains](#sobrecarga-na-ativação-e-desativação-dos-grains)

### [Clusterização, persistência e reminders no Silo](#5-clusterização,-persistência-e-reminders-no-silo)

- [Pacotes necessários para o projeto do Silo com clusterização ADO.NET](#pacotes-necessários-para-o-projeto-do-silo-com-clusterização-adonet)
- [Pacotes necessários para o projeto do Silo com persistência ADO.NET](#pacotes-necessários-para-o-projeto-do-silo-com-persistência-adonet)
- [Pacotes necessários para o projeto do Silo com reminders ADO.NET](#pacotes-necessários-para-o-projeto-do-silo-com-reminders-adonet)
- [Pacotes necessários para o projeto do Client com clusterização ADO.NET](#pacotes-necessários-para-o-projeto-do-client-com-clusterização-adonet)
- [Bootstrap do Silo em ambiente com clusterização, persistência e reminders ADO.NET](#bootstrap-do-silo-em-ambiente-com-clusterização,-persistência-e-reminders-adonet)
- [Bootstrap do Silo em ambiente com múltiplas fontes de persistência ADO.NET](#bootstrap-do-silo-em-ambiente-com-múltiplas-fontes-de-persistência-adonet)
- [Bootstrap do Client em ambiente com clusterização ADO.NET](#bootstrap-do-client-em-ambiente-com-clusterização-adonet)
- [Grains com uma fonte de persistência](#grains-com-uma-fonte-de-persistência)
- [Grains com múltiplas fontes de persistência](#grains-com-múltiplas-fontes-de-persistência)

### [Timers e Reminders nos Grains](#6-timers-e-reminders-nos-grains)

- [Grains com Timers para tarefas repetidas](#grains-com-timers-para-tarefas-repetidas)
- [Grains com Reminders para tarefas repetidas](#grains-com-reminders-para-tarefas-repetidas)

### [Injeções de dependência no Orleans](#7-injeções-de-dependência-no-orleans)

- [Injeção de dependência no Silo](#injeção-de-dependência-no-silo)
- [Injeção de dependência no Client](#injeção-de-dependência-no-client)

### Manipulação e gerenciamento de Grains nos Silos

- [Chamando Grains dentro de outros Grains](#chamando-grains-dentro-de-outros-grains)
- [Marcando Grains como Reentrant](#marcando-grains-como-reentrant)

## 1. Introdução

Direto ao ponto, aqui vamos adicionar trechos de código numa espécie de cola rápida para as situações repetitivas que o Orleans tem - não haverá explicações sobre o funcionamento dos códigos aqui.

## 2. Básico

#### Pacotes necessários para o projeto de interfaces de Grains

- Pacote nuget **Microsoft.Orleans.Core.Abstractions**
- Pacote nuget **Microsoft.Orleans.CodeGenerator.MSBuild**

#### Pacotes necessários para o projeto de implementações de Grains

- Pacote nuget **Microsoft.Orleans.Core.Abstractions**
- Pacote nuget **Microsoft.Orleans.CodeGenerator.MSBuild**
- Projeto de **Interfaces dos Grains**

#### Pacotes necessários para o projeto do Silo

- Pacote nuget **Microsoft.Orleans.Server**
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

#### Pacotes necessários para o projeto do Client

- Pacote nuget **Microsoft.Orleans.Core**
- Projeto de **Interfaces dos Grains**

#### [Bootstrap do Silo em ambiente local](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/01%20-%20Bootstrap%20do%20Silo%20em%20ambiente%20local)

#### [Bootstrap do Client em ambiente local](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/02%20-%20Bootstrap%20do%20Client%20em%20ambiente%20local)

## 3. Básico com Logging

#### Pacotes necessários para o projeto do Silo com logging no console

- Pacote nuget **Microsoft.Extensions.Logging.Console**
- Pacote nuget **Microsoft.Orleans.Server**
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

#### Pacotes necessários para o projeto do Client com logging no console

- Pacote nuget **Microsoft.Extensions.Logging.Console**
- Pacote nuget **Microsoft.Orleans.Core**
- Projeto de **Interfaces dos Grains**

#### [Bootstrap do Silo em ambiente local com logging no console](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/03%20-%20Bootstrap%20do%20Silo%20em%20ambiente%20local%20com%20logging%20no%20console)

#### [Bootstrap do Client em ambiente local com logging no console](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/04%20-%20Bootstrap%20do%20Client%20em%20ambiente%20local%20com%20logging%20no%20console)

## 4. Configuração básica dos Grains

#### [Implementando e recuperando chaves primárias dos Grains](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/05%20-%20Implementando%20e%20recuperando%20chaves%20prim%C3%A1rias%20dos%20Grains)

#### [Sobrecarga na ativação e desativação dos Grains](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/06%20-%20Sobrecarga%20na%20ativa%C3%A7%C3%A3o%20e%20desativa%C3%A7%C3%A3o%20dos%20Grains)

## 5. Clusterização, persistência e reminders no Silo

#### Pacotes necessários para o projeto do Silo com clusterização ADO.NET

- Pacote nuget **Microsoft.Orleans.Server**
- Pacote nuget **Microsoft.Orleans.Clustering.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

#### Pacotes necessários para o projeto do Silo com persistência ADO.NET

- Pacote nuget **Microsoft.Orleans.Server**
- Pacote nuget **Microsoft.Orleans.Persistence.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

#### Pacotes necessários para o projeto do Silo com reminders ADO.NET

- Pacote nuget **Microsoft.Orleans.Server**
- Pacote nuget **Microsoft.Orleans.Reminders.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

#### Pacotes necessários para o projeto do Client com clusterização ADO.NET

- Pacote nuget **Microsoft.Orleans.Core**
- Pacote nuget **Microsoft.Orleans.Clustering.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**

#### [Bootstrap do Silo em ambiente com clusterização, persistência e reminders ADO.NET](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/07%20-%20Bootstrap%20do%20Silo%20em%20ambiente%20com%20clusteriza%C3%A7%C3%A3o%2C%20persist%C3%AAncia%20e%20reminders%20ADO.NET)

#### [Bootstrap do Silo em ambiente com múltiplas fontes de persistência ADO.NET](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/08%20-%20Bootstrap%20do%20Silo%20em%20ambiente%20com%20m%C3%BAltiplas%20fontes%20de%20persist%C3%AAncia%20ADO.NET)

#### [Bootstrap do Client em ambiente com clusterização ADO.NET](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/09%20-%20Bootstrap%20do%20Client%20em%20ambiente%20com%20clusteriza%C3%A7%C3%A3o%20ADO.NET)

#### [Grains com uma fonte de persistência](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/10%20-%20Grains%20com%20uma%20fonte%20de%20persist%C3%AAncia)

#### [Grains com múltiplas fontes de persistência](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/11%20-%20Grains%20com%20m%C3%BAltiplas%20fontes%20de%20persist%C3%AAncia)

## 6. Timers e Reminders nos Grains

#### [Grains com Timers para tarefas repetidas](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/12%20-%20Grains%20com%20Timers%20para%20tarefas%20repetidas)

#### [Grains com Reminders para tarefas repetidas](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/13%20-%20Grains%20com%20Reminders%20para%20tarefas%20repetidas)

## 7. Injeções de dependência no Orleans

#### [Injeção de dependência no Silo](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/14%20-%20Inje%C3%A7%C3%A3o%20de%20depend%C3%AAncia%20no%20Silo)

#### [Injeção de dependência no Client](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/15%20-%20Inje%C3%A7%C3%A3o%20de%20depend%C3%AAncia%20no%20Client)


## 8. Manipulação e gerenciamento de Grains nos Silos

#### [Chamando Grains dentro de outros Grains](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/16%20-%20Chamando%20Grains%20dentro%20de%20outros%20Grains)
#### [Marcando Grains como Reentrant](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/17%20-%20Marcando%20Grains%20como%20Reentrant)

