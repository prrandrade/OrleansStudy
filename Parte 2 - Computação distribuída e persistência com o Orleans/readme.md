# Parte 2 - Computação distribuída e persistência ADO.NET com o Orleans

- [Introdução](#1-introdução)
- [Persistência e rede para organizar os Silos](#2-persistência-e-rede-para-organizar-os-silos)
- [Entendendo a persistência do Orleans](#3-entendendo-a-persistência-do-orleans)
- [Scripts de preparação de persistência ADO.NET no Orleans](#4-scripts-de-preparação-de-persistência-adonet-no-orleans)
- [Projeto BasicClusterAdonet](#5-projeto-basicclusteradonet)
- [Após o projeto BasicClusterAdoNet](#6-após-o-projeto-basicclusteradonet)
- [Projeto BasicClusterAdoNetMultipleSilos](#7-projeto-basicclusteradonetmultiplesilos)
- [Após o projeto BasicClusterAdoNetMultipleSilos](#8-após-o-projeto-basicclusteradonetmultiplesilos)
- [Projeto SiloReconnection](#9-projeto-siloreconnection)
- [Após o projeto SiloReconnection](#10-após-o-projeto-siloreconnection)
- [Projeto ObjectPersistence](#11-projeto-objectpersistence)
- [Após o projeto ObjectPersistence](#12-após-o-projeto-objectpersistence)
- [Projeto GrainTimers](#13-projeto-graintimers)
- [Após o projeto GrainTimers](#14-após-o-projeto-graintimers)
- [Projeto GrainReminders](#15-projeto-grainreminders)
- [Após o projeto GrainReminders](#16-após-o-projeto-grainreminders)
- [Sumário dos projetos](#17-sumário-dos-projetos)
- [Conclusão](#18-conclusão)

# 1. Introdução

Como explicado anteriormente, o que fizemos até aqui foi conhecer os aspectos mais básicos do Orleans, mas usar apenas estes aspectos não traz vantagens de fato a um ambiente de produção - veja, se estamos apenas fazendo chamadas cliente-servidor, por mais que o uso do Orleans disfarce isso de forma que parece que estamos chamando métodos locais, o funcionamento não é muito diferente de uma API.

Mas o Orleans não foi feito para substituir uma API, o Orleans foi feito para ser usado em computação distribuída. E agora vamos aprender como podemos distribuir o processamento dos **Grains** de forma que as requisições feitas pelos **Clients** serão distribuídas entre **Silos**.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 2. Persistência e rede para organizar os Silos

Já sabemos que a o que realmente precisa ser executado de forma distribuída são os **Silos** - e um conjunto de **Silos** que lidam com os mesmos **Grains** é um **Cluster**. Mas para que isso funcione, os **Silos** precisam conversar entre si e se organizem de forma a não 'bater cabeça', o problema mais comum
quando queremos que o mesmo sistema seja executado ao mesmo tempo em mais de uma máquina.

O Orleans resolve isso de forma estupidamente simples: persistência de dados. Precisamos de uma base de dados unificada a qual todos os **Silos** terão acesso para armazenar informações sobre eles. Ao mesmo tempo, os **Silos** utilizam duas portas de rede para o tráfego de informações: uma porta onde eles conversam entre si e OUTRA porta onde os **Clients** se conectam para ativar e utilizar os **Grains**.

Resumo da história: todas as máquinas que hospedam **Silos** precisam ter acesso a uma **mesma base de dados** e também acesso de **rede entre elas** - pelo menos na porta específica de cada **Silo** - nem precisa ser a mesma! Isso merece ser destacado principalmente em ambientes corporativos, onde é muito fácil esquecer de pedir acesso de uma máquina a outra por uma porta específica. Vale destacar também que esta persistência **não tem relação com nenhuma lógica de negócio**, estamos falando do **funcionamento interno do Orleans**.

Vale destacar também que os **Clients** acessam a base de dados para definir qual **Silo** será usado para o processamento do **Grain**. Portanto, os **Clients** precisam ter acesso a todos os **Silos** e também a base de dados.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 3. Entendendo a persistência do Orleans

Antes de tudo, vale destacar que os **Silos** do Orleans utilizam a persistência para três tipos de operações.

-   **Clustering**: A persistência é utilizada para que os **Silos** do **Cluster** se comuniquem entre si; é a persistência que falamos até aqui.

-   **Persistence**: Quando os **Grains** precisam armazenar/carregar objetos, um sistema próprio de persistência pode (e em algumas situações, deve) ser utilizado.

-   **Reminders**: **Grains** podem ter tarefas agendadas que serão executadas mesmo quando eles não estiverem ativados. As informações destas tarefas agendadas precisam ser persistidas em algum lugar para que a funcionalidade seja executada de forma correta.

Não precisamos configurar os três aspectos de persistência simultaneamente, eles trabalham de forma independente - nem precisamos usar o mesmo banco de dados para cada um dos aspectos!

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 4. Scripts de preparação de persistência ADO.NET no Orleans

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

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 5. Projeto BasicClusterAdoNet

O [projeto BasicClusterAdoNet][05-BasicClusterAdoNet] é uma versão revisitada do [HelloWorld][01-HelloWorld], só que agora com os conceitos de configuração de persistência e **cluster** gerenciados via ADO.NET.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 6. Após o projeto BasicClusterAdoNet

Conseguimos montar o primeiro cenário real com um **Client** e um **Silo** de forma distribuída. Usando uma base de dados com ADO.NET, o Orleans permite que a comunicação entre as diferentes partes seja indireta o suficiente para que mais **Silos** possam ser carregados sem precisar reiniciar os **Clients**. Agora vamos montar o primeiro cenário real de computação distribuída - mas vale destacar algo antes!

Repare que, no [projeto BasicClusterAdoNet][05-BasicClusterAdoNet], as configurações de portas são feitas no **Silo**. É importante destacar isso porque os **Silos** foram pensados em executar em máquinas diferentes - é computação distribuída, afinal de contas! Sem entrar no mérito de onde os **Silos** serão executados em produção, (máquinas físicas, máquina virtuais, kubernetes, etc), para fins de estudo é totalmente possível executar mais de um **Silo** na mesma máquina física. Para tal precisamos fazer que com cada **Silo** seja executado em portas diferentes.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 7. Projeto BasicClusterAdoNetMultipleSilos

O [projeto BasicClusterAdoNetMultipleSilos][06-BasicClusterAdoNetMultipleSilos] coloca o conceito de múltiplos **Silos** na mesma máquina em prática, vamos conseguir subir dois ou mais **Silos** na mesma máquina e o **Client** os usará de forma equalizada, priorizando o **Silo** menos usado no momento - computação distribuída na prática!

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 8. Após o projeto BasicClusterAdoNetMultipleSilos

Quando você monta um Cluster com mais de um **Silo**, pode haver situações que o **Silo** escolhido para o processamento do **Grain** acabe não devolvendo a resposta num tempo adequado, ou simplesmente perca a conexão. Neste caso, precisamos preparar o **Client** para que ele saiba o que fazer caso não aja resposta. Já fizemos isso de forma básica no [projeto BasicClusterAdoNetMultipleSilos][06-BasicClusterAdoNetMultipleSilos], mas vamos ver este comportamento mais a fundo.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 9. Projeto SiloReconnection

O [projeto SiloReconnection][07-SiloReconnection] apresenta formas de lidar com casos onde o **Silo** simplesmente não responde a uma chamada de um **Grain** feita pelo **Client** e/ou o tempo de resposta não é adequado. Basicamente são métodos de extensão do próprio Orleans que lidam com estes casos.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 10. Após o projeto SiloReconnection

Agora que já sabemos como podemos fazer com que o **Client** perceba quando um **Silo** não respondeu a chamada feita a um **Grain** a tempo, chegou a  hora de aprendermos como persistir objetos no lado do servidor, ou seja, como os **Grains** podem salvar objetos na base de dados e recuperá-los posteriormente.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 11. Projeto ObjectPersistence

O [projeto ObjectPersistence][08-ObjectPersistence] mostra como configurar os **Silos** e os **Grains** para que a persistência de objetos possa ser utilizada corretamente. Sem muitas configurações, cada **Grain** pode serializar e desserializar objetos de forma individual, separados por chave primária.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 12. Após o projeto ObjectPersistence

Note que, em absolutamente todos os casos que vimos até agora, o **Grain** nunca age, ele apenas reage em relação às chamadas feitas por algum **Client**. Só que é totalmente possível que o próprio **Grain** consiga realizar procedimentos agendados sem precisar ser provocado (a não ser para o próprio agendamento, claro). Existem duas funcionalidades que permitem este comportamento no Orleans: **Timers** e **Reminders** (tarefas agendadas). Vamos primeiramente conhecer os **Timers**, com funcionamento mais simples.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 13. Projeto GrainTimers

No [projeto GrainTimers][09-GrainTimers], vamos aprender como ativar e desativar **Timers** em **Grains** para a repetição de tarefas sem precisar de estímulo externo (como o **Client**).

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 14. Após o projeto GrainTimers

Agora que já sabemos como criar repetições leves em **Grains** através dos **Timers**, vamos conhecer mais a fundo o uso dos **Reminders**, cujo objetivo básico é o mesmo mas de forma bem mais robusta - tanto que os **Reminders** tem até sua própria persistência para garantir que a tarefa agendada será executada!

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 15. Projeto GrainReminders

O [projeto GrainReminders][10-GrainReminders] mostra como podemos criar, atualizar e apagar tarefas agendadas, além de configurar os **Silos** para persistir as configurações de uma tarefa agendada, já que está nem precisa do **Grain** ativado para que o **Reminder** seja executado.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 16. Após o projeto GrainReminders

Com isso, já conseguimos agendar tarefas nos **Grains** usando **Timers** e **Reminders**, com destaque para este pois precisa de uma infraestrutura mais robusta para funcionar - em troca, garante o disparo da tarefa agendada mesmo se o **Grain** não estiver ativado.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 17. Sumário dos projetos

- Num ambiente corporativo, a organização entre **Silos** e **Clients** pode ser feita através de bases de dados.
- Diferentes bases de dados podem ser usadas para clusterização, tarefas agendadas e persistência de objetos. Você não precisa mexer nas bases de dados já existentes e nem precisa usar a mesma base de dados para as três funcionalidades - mas lembre-se de executar os scripts de preparação das bases!
- **Clients** não sabem onde estão os **Silos** e nem precisam se conectar a eles. Todo este meio de campo é feito através da base de dados de clusterização, o que é uma das vantagens do Orleans. Você consegue subir mais **Silos** no mesmo **Cluster** sem nem precisar reiniciar **Clients**!
- Podemos executar mais de um **Silo** na mesma máquina sem problemas, desde que cada **Silo** use portas específicas para comunicação entre si e entre **Clients**.
- Todas as configurações de conexão são feitas apenas nos **Silos** e armazenadas na base de dados escolhida. Justamente por isso o **Client** apenas se conecta apenas à base de dados.
- Não é necessária nenhuma configuração nos **Clients** para distribuir as chamadas dos **Grains** em diferentes **Silos**. A partir que mais de um **Silo** está no mesmo **Cluster**, a computação distribuída já funciona.
- O método ``WaitWithThrow`` permite esperar por um tempo finito pela resposta de uma chamada, permitindo programar outros comportamentos.
- Podemos usar métodos de extensão do Orleans, disponíveis no **Client**, para lidar com as ocasiões onde o **Silo** não responde a chamada de um **Grain**.
- O método de extensão `WithTimeout` é mais indicado para não travar a thread principal, se ela está ocupada processando outras chamadas.
- O Orleans parte do pressuposto de que um método de um **Grain** deve devolver a resposta em até 200ms antes de escrever um log de alerta. Isso não bloqueia a execução de nada, claro, mas mostra que o Orleans é pensado para o uso de métodos com resposta rápida.
- Quando usamos a persistência do Orleans, estamos falando na serialização e desserialização de objetos!
- Podemos usar mais de uma base de dados para persistir diferentes objetos. Neste caso, cada base de dados de persistência precisa ter um nome interno específico para identificação.
- A serialização de objetos pode ser feita com XML, JSON ou binário (que é o padrão).
- Mais de um objeto pode ser persistido por **Grain**.
- A persistência é individualizada por chave primária do **Grain** - objetos de um **Grain** não afetam objetos de outro.
- Todos os objetos são carregados assim que o **Grain** é ativado, mas nada é salvo automaticamente, lembre-se disso!
- **Timers** não precisam de configuração extra e podem ser usados para tarefas corriqueiras e/ou bem frequentes.
- **Timers** ficam armazenados em memória, eles são zerados quando o **Grain** é desativado no **Silo**.
- **Reminders** precisam de persistência própria para funcionarem corretamente.
- **Reminders** podem ser criados/atualizados com a mesma referência.
- Uma vez que um **Reminder** foi cadastrado, não há necessidade de manter o **Grain** ativado, pois o Orleans já faz a ativação do mesmo (se necessário) para o processamento do **Reminder**.
- Como precisa-se de uma infraestrutura maior para a execução dos **Reminders**, há um prazo mínimo espera de 1 minuto para entre as execuções de um **Reminder**.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

# 18. Conclusão

Após passar por todos os exemplos, conseguimos cobrir os primeiros aspectos que fazem do Orleans não apenas uma forma mais elegante de substituir APIs, mas um verdadeiro framework de computação distribuída.

- Conseguimos de fato usar a computação distribuída de forma transparente, de forma que o **Client** nem sabe quantos **Silos** estão de pé no **Cluster** no momento em que a chamada de um **Grain** é feita.
- Sem nenhuma configuração, as chamadas aos **Grains** são distribuídas entre os **Silos** da forma quais igualitária possível.
- Os **Grains** podem ter estados, persistidos na base de dados de forma separada de todos o resto da lógica de negócio
- Tarefas agendadas podem ser realizadas de forma semelhante por **Timers** e **Reminders**, sendo que estes exigem uma infraestrutura de persistência, mas conseguem disparar as tarefas mesmo se o **Grain** não estiver ativado.

<div align="right">
	
[Voltar](#computação-distribuída-e-persistência-adonet-com-o-orleans)

</div>

[01-HelloWorld]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/01-HelloWorld
[05-BasicClusterAdoNet]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/05-BasicClusterAdoNet
[06-BasicClusterAdoNetMultipleSilos]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/06-BasicClusterAdoNetMultipleSilos
[07-SiloReconnection]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/07-SiloReconnection
[08-ObjectPersistence]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/08-ObjectPersistence
[09-GrainTimers]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/09-GrainTimers
[10-GrainReminders]: https://github.com/prrandrade/OrleansStudy/tree/master/Projetos/10-GrainReminders
