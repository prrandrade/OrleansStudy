# Parte 2 - Computação distribuída e persistência com o Orleans

- [Introdução](#introdução)
- [Persistência e rede para organizar os Silos](#persistência-e-rede-para-organizar-os-silos)
- [Entendendo a persistência do Orleans](#entendendo-a-persistência-do-orleans)
- [Configurando a persistência no Orleans](#configurando-a-persistência-no-orleans)

# Introdução

Como explicado anteriormente, o que fizemos até aqui foi conhecer os aspectos mais básicos do Orleans, mas usar apenas estes aspectos não traz vantagens de fato a um ambiente de produção - veja, se estamos apenas fazendo chamadas cliente-servidor, por mais que o uso do Orleans disfarce isso de forma que parece que estamos chamando métodos locais, o funcinamento não se torma muito diferente de uma API.

Mas o Orleans não foi feito para substituir uma API, o Orleans foi feito para ser usado em computação distribuída. E agora vamos aprender como podemos distribuir o processamento dos **Grains** de forma que as requisições feitas pelos **Clients** serão distrubuídas entre **Silos**.

# Persistência e rede para organizar os Silos

Já sabemos que a o que realmente precisa ser executado de forma distribuída são os **Silos** - e um conjunto de **Silos** que lidam com os mesmos **Grains** é um **Cluster**. Mas para que isso funcione, os **Silos** precisam conversar entre si e se organizem de forma a não 'bater cabeça', o problema mais comum quando queremos que o mesmo sistema seja executado ao mesmo tempo em mais de uma máquina.

O Orleans resolve isso de forma estupidamente simples: persistência de dados. Precisamos de uma base de dados unificada a qual todos os **Silos** terão acesso para armazenar informações sobre eles. Ao mesmo tempo, os **Silos** utilizam duas portas de rede para o tráfego de informações: uma porta onde eles conversam entre si e OUTRA porta onde os **Clients** se conectam para ativar e utilizar os **Grains**.

Resumo da história: todas as máquinas que hospedam **Silos** precisam ter acesso a uma **mesma base de dados** e também acesso de **rede entre elas** - pelo menos em uma porta específica. Isso merece ser destacado principalmente em ambientes corporativos, onde é muito fácil esquecer de pedir acesso de uma máquina a poutra por uma porta específica.

# Entendendo a persistência do Orleans

Antes de tudo, vale destacar que os **Silos** do Orleans utilizam a persistência para três tipos de operações.

- **Clustering**: A persistência é utilizada para que os **Silos** do **Cluster** se comuniquem entre si; é a persistência que falamos até aqui.

- **Persistence**: Quando os **Grains** percisam armazenar/carregar objetos, um sistema próprio de persistência pode (e em algumas situações, deve) ser utilizado.

- **Remimders**: **Grains** podem ter tarefas agendadas que serão executadaas mesmo quando eles não estiverem ativados. As informações destas tarefas agendadas precisa ser persistida em algum lugar para que a funcionalidade seja executada de forma correta.

Não precisamos configurar os três aspectos de persistência simultaneamente, eles trabalham de forma independente.

# Configurando a persistência no Orleans


