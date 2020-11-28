# Parte 2 - Computação distribuída e persistência ADO.NET com o Orleans

- [Introdução](#introdução)
- [Persistência e rede para organizar os Silos](#persistência-e-rede-para-organizar-os-silos)
- [Entendendo a persistência do Orleans](#entendendo-a-persistência-do-orleans)
- [Scripts de preparação de persistência no Orleans](#scripts-de-preparação-de-persistência-no-orleans)
- [Projeto BasicCluster](#projeto-basiccluster)
- [Após o projeto BasicCluster](#após-o-projeto-basiccluster)

# Introdução

Como explicado anteriormente, o que fizemos até aqui foi conhecer os aspectos mais básicos do Orleans, mas usar apenas estes aspectos não traz vantagens de fato a um ambiente de produção - veja, se estamos apenas fazendo chamadas cliente-servidor, por mais que o uso do Orleans disfarce isso de forma que parece que estamos chamando métodos locais, o funcionamento não é muito diferente de uma API.

Mas o Orleans não foi feito para substituir uma API, o Orleans foi feito para ser usado em computação distribuída. E agora vamos aprender como podemos distribuir o processamento dos **Grains** de forma que as requisições feitas pelos **Clients** serão distribuídas entre **Silos**.

# Persistência e rede para organizar os Silos

Já sabemos que a o que realmente precisa ser executado de forma distribuída são os **Silos** - e um conjunto de **Silos** que lidam com os mesmos **Grains** é um **Cluster**. Mas para que isso funcione, os **Silos** precisam conversar entre si e se organizem de forma a não 'bater cabeça', o problema mais comum
quando queremos que o mesmo sistema seja executado ao mesmo tempo em mais de uma máquina.

O Orleans resolve isso de forma estupidamente simples: persistência de dados. Precisamos de uma base de dados unificada a qual todos os **Silos** terão acesso para armazenar informações sobre eles. Ao mesmo tempo, os **Silos** utilizam duas portas de rede para o tráfego de informações: uma porta onde eles conversam entre si e OUTRA porta onde os **Clients** se conectam para ativar e utilizar os **Grains**.

Resumo da história: todas as máquinas que hospedam **Silos** precisam ter acesso a uma **mesma base de dados** e também acesso de **rede entre elas** - pelo menos em uma porta específica. Isso merece ser destacado principalmente em ambientes corporativos, onde é muito fácil esquecer de pedir acesso de uma
máquina a outra por uma porta específica. Vale destacar também que esta persistência **não tem relação com nenhuma lógica de negócio**, estamos falando do **funcionamento interno do Orleans**.

Vale destacar também que os **Clients** acessam a base de dados para definir qual **Silo** será usado para o processamento do **Grain**. Portanto, os **Clients** precisam ter acesso a todos os **Silos** e também a base de dados.

# Entendendo a persistência do Orleans

Antes de tudo, vale destacar que os **Silos** do Orleans utilizam a persistência para três tipos de operações.

-   **Clustering**: A persistência é utilizada para que os **Silos** do **Cluster** se comuniquem entre si; é a persistência que falamos até aqui.

-   **Persistence**: Quando os **Grains** precisam armazenar/carregar objetos, um sistema próprio de persistência pode (e em algumas situações, deve) ser utilizado.

-   **Reminders**: **Grains** podem ter tarefas agendadas que serão executadas mesmo quando eles não estiverem ativados. As informações destas tarefas agendadas precisam ser persistidas em algum lugar para que a funcionalidade seja executada de forma correta.

Não precisamos configurar os três aspectos de persistência simultaneamente, eles trabalham de forma independente - nem precisamos usar o mesmo banco de dados para cada um dos aspectos!

# Scripts de preparação de persistência ADO.NET no Orleans

[Na documentação oficial do Microsoft Orleans](https://dotnet.github.io/orleans/docs/host/configuration_guide/adonet_configuration.html), encontramos os quatro tipos de banco de dados que podem ser usados para a persistência interna do Orleans via ADO.NET, juntamente com os scripts que devem ser adicionados ao projeto do **Silo**:

-   **SQL Server**, que exige a instalação do pacote nuget **System.Data.SqlClient** e é internamente identificado como **System.Data.SqlClient**.

-   **MySQL** ou **MariaDB**, que exige a instalação do pacote nuget **MySql.Data** e é internamente identificado como **MySql.Data.MySqlClient**.

-   **PostgreSQL**, que exige a instalação do pacote nuget **Npgsql** e é identificado internamente como **Npgsql**.

-   **Oracle**, que exige a instalação do pacote nuget **ODP.net** e é identificado internamente como **Oracle.DataAccess.Client**.

No mesmo link, você consegue obter os scripts (de cada banco de dados) que devem ser executados anteriormente nas bases de dados que serão acessadas pelos **Silos** para o uso de persistência. Estes estão divididos em quatro categorias:

-   **Main**: são os scripts obrigatórias que devem estar presentes em todos os casos.

-   **Clustering**: são os scripts que devem ser executados quando queremos executar múltiplos **Silos** no mesmo **Cluster** (computação distribuída, afinal de contas).

-   **Persistence**: são os scripts que devem ser executados quando queremos usar o mecanismo de persistência de objetos dentro dos **Grains**.

-   **Reminders**: São os scripts que devem ser executados quando precisamos usar o mecanismo de tarefas agendadas dos **Grains**.

Por exemplo, se o banco de dados de persistência do Orleans for o SQL Server e o mecanismo de Clustering for usado, precisamos baixar os scripts **SQLServer-Main.sql** e **SQLServer-Clustering.sql**. Já se o banco de dados for o Oracle e os mecanismos de tarefas agendadas e persistência de objetos forem usados, precisamos baixar os scripts **Oracle-Main.sql**, **Oracle-Reminders.sql** e **Oracle-Persistence.sql**.

E se usarmos mais de um banco de dados para tarefas diferentes, os scripts **Main** de ambas as bases de dados precisam ser executados, não se esqueça disso!

# Projeto BasicCluster

O [projeto BasicCluster][basic-cluster] é uma versão revisitada do[HelloWorld][helloworld], só que agora com os conceitos de configuração de persistência e **cluster** vistos na ótica do ADO.NET.

# Após o projeto BasicCluster

Conseguimos montar o primeiro cenário real com um **Client** e um **Silo** de forma distribuída. Usando uma base de dados com ADO.NET, o Orleans permite que a comunicação entre as diferentes partes seja indireta o suficiente para que mais **Silos** possam ser carregados sem precisar reiniciar os **Clients**.

[helloworld]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/01-HelloWorld
[basic-cluster]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/05-BasicCluster

