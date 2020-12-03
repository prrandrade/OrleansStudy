# Parte 2 - Computação distribuída e persistência ADO.NET com o Orleans

- [Introdução](#introdução)
- [Persistência e rede para organizar os Silos](#persistência-e-rede-para-organizar-os-silos)
- [Entendendo a persistência do Orleans](#entendendo-a-persistência-do-orleans)
- [Scripts de preparação de persistência ADO.NET no Orleans](#scripts-de-preparação-de-persistência-adonet-no-orleans)
- [Projeto BasicClusterAdonet](#projeto-basicclusteradonet)
- [Após o projeto BasicClusterAdoNet](#após-o-projeto-basicclusteradonet)
- [Projeto BasicClusterAdoNetMultipleSilos](#projeto-basicclusteradonetmultiplesilos)
- [Após o projeto BasicClusterAdoNetMultipleSilos](#após-o-projeto-basicclusteradonetmultiplesilos)
- [Projeto SiloReconnection](#projeto-siloreconnection)
- [Após o projeto SiloReconnection](#após-o-projeto-siloreconnection)
- [Projeto ObjectPersistence](#projeto-objectpersistence)
- [Após o projeto ObjectPersistence](#após-o-projeto-objectpersistence)
- [Projeto GrainTimers](#projeto-graintimers)

# Introdução

Como explicado anteriormente, o que fizemos até aqui foi conhecer os aspectos mais básicos do Orleans, mas usar apenas estes aspectos não traz vantagens de fato a um ambiente de produção - veja, se estamos apenas fazendo chamadas cliente-servidor, por mais que o uso do Orleans disfarce isso de forma que parece que estamos chamando métodos locais, o funcionamento não é muito diferente de uma API.

Mas o Orleans não foi feito para substituir uma API, o Orleans foi feito para ser usado em computação distribuída. E agora vamos aprender como podemos distribuir o processamento dos **Grains** de forma que as requisições feitas pelos **Clients** serão distribuídas entre **Silos**.

# Persistência e rede para organizar os Silos

Já sabemos que a o que realmente precisa ser executado de forma distribuída são os **Silos** - e um conjunto de **Silos** que lidam com os mesmos **Grains** é um **Cluster**. Mas para que isso funcione, os **Silos** precisam conversar entre si e se organizem de forma a não 'bater cabeça', o problema mais comum
quando queremos que o mesmo sistema seja executado ao mesmo tempo em mais de uma máquina.

O Orleans resolve isso de forma estupidamente simples: persistência de dados. Precisamos de uma base de dados unificada a qual todos os **Silos** terão acesso para armazenar informações sobre eles. Ao mesmo tempo, os **Silos** utilizam duas portas de rede para o tráfego de informações: uma porta onde eles conversam entre si e OUTRA porta onde os **Clients** se conectam para ativar e utilizar os **Grains**.

Resumo da história: todas as máquinas que hospedam **Silos** precisam ter acesso a uma **mesma base de dados** e também acesso de **rede entre elas** - pelo menos na porta específica de cada **Silo** - nem precisa ser a mesma! Isso merece ser destacado principalmente em ambientes corporativos, onde é muito fácil esquecer de pedir acesso de uma máquina a outra por uma porta específica. Vale destacar também que esta persistência **não tem relação com nenhuma lógica de negócio**, estamos falando do **funcionamento interno do Orleans**.

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

# Projeto BasicClusterAdoNet

O [projeto BasicClusterAdoNet][05-BasicClusterAdoNet] é uma versão revisitada do [HelloWorld][01-HelloWorld], só que agora com os conceitos de configuração de persistência e **cluster** gerenciados via ADO.NET.

# Após o projeto BasicClusterAdoNet

Conseguimos montar o primeiro cenário real com um **Client** e um **Silo** de forma distribuída. Usando uma base de dados com ADO.NET, o Orleans permite que a comunicação entre as diferentes partes seja indireta o suficiente para que mais **Silos** possam ser carregados sem precisar reiniciar os **Clients**. Agora vamos montar o primeiro cenário real de computação distribuída - mas vale destacar algo antes!

Repare que, no [projeto BasicClusterAdoNet][05-BasicClusterAdoNet], as configurações de portas são feitas no **Silo**. É importante destacar isso porque os **Silos** foram pensados em executar em máquinas diferentes - é computação distribuída, afinal de contas! Sem entrar no mérito de onde os **Silos** serão executados em produção, (máquinas físicas, máquina virtuais, kubernetes, etc), para fins de estudo é totalmente possível executar mais de um **Silo** na mesma máquina física. Para tal precisamos fazer que com cada **Silo** seja executado em portas diferentes.

# Projeto BasicClusterAdoNetMultipleSilos

O [projeto BasicClusterAdoNetMultipleSilos][06-BasicClusterAdoNetMultipleSilos] coloca o conceito de múltiplos **Silos** na mesma máquina em prática, vamos conseguir subir dois ou mais **Silos** na mesma maquina e o **Client** os usará de forma equalizada, priorizando o **Silo** menos usado no momento - computação distribuída na prática!

# Após o projeto BasicClusterAdoNetMultipleSilos

Quando você monta um Cluster com mais de um **Silo**, pode haver situações que o **Silo** escolhido para o processamento do **Grain** acabe não devolvendo a resposta num tempo adequado, ou simplesmente perca a conexão. Neste caso, precisamos preparar o **Client** para que ele saiba o que fazer caso não aja resposta. Já fizemos isso de forma básica no [projeto BasicClusterAdoNetMultipleSilos][06-BasicClusterAdoNetMultipleSilos], mas vamos ver este comportamento mais a fundo.

# Projeto SiloReconnection

O [projeto SiloReconnection][07-SiloReconnection] apresenta formas de lidar com casos onde o **Silo** simplesmente não responde a uma chamada de um **Grain** feita pelo **Client** e/ou o tempo de resposta não é adequado. Basicamente são métodos de extensão do próprio Orleans que lidam com estes casos.

# Após o projeto SiloReconnection

Agora que já sabemos como podemos fazer com que o **Client** perceba quando um **Silo** não respondeu a chamada feita a um **Grain** a tempo, chegoua  hora de aprendermos como persistir objetos no lado do servidor, ou seja, como os **Grains** podem salvar objetos na base de dados e recuperá-los posteriormente.

# Projeto ObjectPersistence

O [projeto ObjectPersistence][08-ObjectPersistence] mostra como configurar os **Silos** e os **Grains** para que a persistência de objetos possa ser utilizada corretamente. Sem muitas configurações, cada **Grain** pode serializar e desserializar objetos de forma individual, separados por chave primária.

# Após o projeto ObjectPersistence

Note que, em absolutamente todos os casos que vimos até agora, o **Grain** nunca age, ele apenas reage em relação às chamadas feitas por algum **Client**. Só que é totalmente possível que o próprio **Grain** consiga realizar procedimentos agendados sem precisar ser provocado (a não ser para o próprio agendamento, claro). Existem duas funcionalidades que permitem este comportamento no Orleans: **Timers** e **Reminders** (tarefas agendadas). Vamos primeiramente conhecer os **Timers**, com funcionamento mais simples.

# Projeto GrainTimers

No [projeto GrainTimers][09-GrainTimers], vamos aprender como ativar e desativar **Timers** em **Grains** para a repetição de tarefas sem precisar de estímulo externo (como o **Client**).

# Após o projeto GrainTimers

Agora que já sabemos como criar repetições leves em **Grains** através dos **Timers**, vamos conhecer mais a fundo o uso dos **Reminders**, cujo objetivo básico é o mesmo mas de forma bem mais robusta - tanto que os **Reminders** tem até sua própria persistência para garantir que a tarefa agendada será executada!


[01-HelloWorld]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/01-HelloWorld

[05-BasicClusterAdoNet]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/05-BasicClusterAdoNet
[06-BasicClusterAdoNetMultipleSilos]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/06-BasicClusterAdoNetMultipleSilos
[07-SiloReconnection]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/07-SiloReconnection
[08-ObjectPersistence]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/08-ObjectPersistence
[09-GrainTimers]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/09-GrainTimers