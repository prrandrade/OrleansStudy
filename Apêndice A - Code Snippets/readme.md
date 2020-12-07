# Apêndice A - Code Snippets

- [Introdução](#introdução)

<!-- Básico -->
- [Pacotes necessários para o projeto de interfaces de Grains](#pacotes-necessários-para-o-projeto-de-interfaces-de-grains)
- [Pacotes necessários para o projeto de implementações de Grains](#pacotes-necessários-para-o-projeto-de-implementações-de-grains)
- [Pacotes necessários para o projeto do Silo](#pacotes-necessários-para-o-projeto-do-silo)
- [Pacotes necessários para o projeto do Client](#pacotes-necessários-para-o-projeto-do-client)
- [Bootstrap do Silo em ambiente local](#bootstrap-do-silo-em-ambiente-local)
- [Bootstrap do Client em ambiente local](#bootstrap-do-client-em-ambiente-local)

<!-- Básico com logging -->
- [Pacotes necessários para o projeto do Silo com logging no console](#pacotes-necessários-para-o-projeto-do-silo-com-logging-no-console)
- [Pacotes necessários para o projeto do Client com logging no console](#pacotes-necessários-para-o-projeto-do-client-com-logging-no-console)
- [Bootstrap do Silo em ambiente local com logging no console](#bootstrap-do-silo-em-ambiente-local-com-logging-no-console)
- [Bootstrap do Client em ambiente local com logging no console](#bootstrap-do-client-em-ambiente-local-com-logging-no-console)

<!-- Básico dos Grains -->
- [Implementando e recuperando chaves primárias dos Grains](#implementando-e-recuperando-chaves-primárias-dos-grains)
- [Sobrecarga na ativação e desativação dos Grains](#sobrecarga-na-ativação-e-desativação-dos-grains)

<!-- Clusterização, persistência e reminders no Silo/CLient -->
- [Pacotes necessários para o projeto do Silo com clusterização ADO.NET](#pacotes-necessários-para-o-projeto-do-silo-com-clusterização-adonet)
- [Pacotes necessários para o projeto do Silo com persistência ADO.NET](#pacotes-necessários-para-o-projeto-do-silo-com-persistência-adonet)
- [Pacotes necessários para o projeto do Silo com reminders ADO.NET](#pacotes-necessários-para-o-projeto-do-silo-com-reminders-adonet)
- [Pacotes necessários para o projeto do Client com clusterização ADO.NET](#pacotes-necessários-para-o-projeto-do-client-com-clusterização-adonet)

<!-- Códigos para Silo e Client com Clusterização, Persistência e Reminders -->
- [Bootstrap do Silo em ambiente com clusterização, persistência e reminders ADO.NET](#bootstrap-do-silo-em-ambiente-com-clusterização,-persistência-e-reminders-adonet)
- [Bootstrap do Client em ambiente com clusterização ADO.NET](#bootstrap-do-client-em-ambiente-com-clusterização-adonet)

<!-- Timers e Reminders nos Grains -->


# Introdução

Direto ao ponto, aqui vamos adicionar trechos de código numa espécie de cola rápida para as situações repetitivas que o Orleans tem - não haverá explicações sobre o funcionamento dos códigos aqui.

# Pacotes necessários para o projeto de interfaces de Grains

- Pacote nuget **Microsoft.Orleans.Core.Abstractions**
- Pacote nuget **Microsoft.Orleans.CodeGenerator.MSBuild**

# Pacotes necessários para o projeto de implementações de Grains

- Pacote nuget **Microsoft.Orleans.Core.Abstractions**
- Pacote nuget **Microsoft.Orleans.CodeGenerator.MSBuild**
- Projeto de **Interfaces dos Grains**

# Pacotes necessários para o projeto do Silo

- Pacote nuget **Microsoft.Orleans.Server**
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Client

- Pacote nuget **Microsoft.Orleans.Core**
- Projeto de **Interfaces dos Grains**

# Bootstrap do Silo em ambiente local

- [Siga o link](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/01%20-%20Bootstrap%20do%20Silo%20em%20ambiente%20local)

# Bootstrap do Client em ambiente local

- [Siga o link](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/02%20-%20Bootstrap%20do%20Client%20em%20ambiente%20local)

# Pacotes necessários para o projeto do Silo com logging no console

- Pacote nuget **Microsoft.Extensions.Logging.Console**
- Pacote nuget **Microsoft.Orleans.Server**
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Client com logging no console

- Pacote nuget **Microsoft.Extensions.Logging.Console**
- Pacote nuget **Microsoft.Orleans.Core**
- Projeto de **Interfaces dos Grains**

# Bootstrap do Silo em ambiente local com logging no console

- [Siga o link](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/03%20-%20Bootstrap%20do%20Silo%20em%20ambiente%20local%20com%20logging%20no%20console)

# Bootstrap do Client em ambiente local com logging no console

- [Siga o link](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/04%20-%20Bootstrap%20do%20Client%20em%20ambiente%20local%20com%20logging%20no%20console)

# Implementando e recuperando chaves primárias dos Grains

- [Siga o link](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/05%20-%20Implementando%20e%20recuperando%20chaves%20prim%C3%A1rias%20dos%20Grains)

# Sobrecarga na ativação e desativação dos Grains

- [Siga o link](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/06%20-%20Sobrecarga%20na%20ativa%C3%A7%C3%A3o%20e%20desativa%C3%A7%C3%A3o%20dos%20Grains)

# Pacotes necessários para o projeto do Silo com clusterização ADO.NET

- Pacote nuget **Microsoft.Orleans.Server**
- Pacote nuget **Microsoft.Orleans.Clustering.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Silo com persistência ADO.NET

- Pacote nuget **Microsoft.Orleans.Server**
- Pacote nuget **Microsoft.Orleans.Persistence.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Silo com reminders ADO.NET

- Pacote nuget **Microsoft.Orleans.Server**
- Pacote nuget **Microsoft.Orleans.Reminders.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Pacotes necessários para o projeto do Client com clusterização ADO.NET

- Pacote nuget **Microsoft.Orleans.Core**
- Pacote nuget **Microsoft.Orleans.Clustering.AdoNet**
- Pacote nuget **System.Data.SqlClient** (se for usar o SQL Server)
- Pacote nuget **MySql.Data** (se for usar o MsSQL ou o MariaDB)
- Pacote nuget **Npgsql** (se for usar o PostgreSQL)
- Pacote nuget **ODP.net** (se for usar o Oracle)
- Projeto de **Interfaces dos Grains**
- Projeto de **Implementações dos Grains**

# Bootstrap do Silo em ambiente com clusterização, persistência e reminders ADO.NET

- [Siga o link](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/07%20-%20Bootstrap%20do%20Silo%20em%20ambiente%20com%20clusteriza%C3%A7%C3%A3o%2C%20persist%C3%AAncia%20e%20reminders%20ADO.NET)

# Bootstrap do Client em ambiente com clusterização ADO.NET

- [Siga o link](https://github.com/prrandrade/OrleansStudy/tree/master/Ap%C3%AAndice%20A%20-%20Code%20Snippets/08%20-%20Bootstrap%20do%20Client%20em%20ambiente%20com%20clusteriza%C3%A7%C3%A3o%20ADO.NET)
