# Projeto GrainReminders

- [Introdução](#1-introdução)
- [Observação rápida sobre a base de dados](#2-observação-rápida-sobre-a-base-de-dados)
- [Quando usar Reminders](#3-quando-usar-reminders)
- [Preparando a base de dados para Reminders](#4-preparando-a-base-de-dados-)
- [Preparando os Silos para Reminders](#5-preparando-os-silos-para-reminders)
- [Como usar Reminders nos Grains](#6-como-usar-reminders-nos-grains)
- [Reminders na prática](#7-reminders-na-prática)
- [Sumário](#8-sumário)

# 1. Introdução

Vamos aprender como usar, ativar e desativar **Reminders** , tarefas agendadas mais pesadas que os **Grains** executar mesmo se não estiverem ativados em nenhum **Silo**.

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 2. Observação rápida sobre a base de dados

Neste exemplo, estou usando uma base de dados local do SQL Server, executada via um [container do Docker][docker-site]. Use a linha de comando que eu separei no repositório [DockerShortcuts][docker-shortcuts].

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 3. Quando usar Reminders

Basicamente falando, **Timers** e **Reminders ** têm o mesmo objetivo: a execução de tarefas agendadas dentro de um **Grain**. mas há uma diferença básica entre eles. **Reminders** continuam sendo executados mesmo quando o **Grain** não está ativado. Isso significa que, de alguma forma, o Orleans precisa guardar as informações dos Reminders para que a execução seja feita (claro, partindo do pressuposto que pelo menos um **Silo** esteja funcional).

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 4. Preparando a base de dados para Reminders

Já falamos [sobre isso anteriormente][readme-parte2], mas os **Silos** e a base de dados precisam ser preparados para persistir as informações sobre os **Reminders**. No caso da base de dados, como este exemplo usa o SQL Server, os scripts que precisam ser executados são **SQLServer-Main.sql** e **SQLServer-Reminders.sql**.

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 5. Preparando os Silos para Reminders

No projeto do **Silo**, você precisa instalar dois pacotes nuget para a correta configuração de persistência dos **Reminders**: o pacote **Microsoft.Orleans.Reminders.AdoNet** e o pacote da base de dados escolhida - neste caso, **System.Data.SqlClient**, pois o base de dados usada é o SQL Server. Simplesmente usamos o método de extensão `UseAdoNetReminderService` na preparação do `SiloHostBuilder`. Sem mistério algum, as configurações que precisam ser feitas é  string de conexão com a base de dados na propriedade `ConnectionString` e o tipo de base de dados em `Invariant`. A identificação da base de dados [já foi explicada anteriormente][readme-parte2].

```csharp
// tarefas agendadas organizadas via banco de dados
.UseAdoNetReminderService(options =>
{
	options.Invariant = "System.Data.SqlClient";
	options.ConnectionString = "Server=localhost;Database=Example;User Id=sa;Password=root@1234";
})
```

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 6. Como usar Reminders nos Grains

Todos os **Grains** que recebem notificação de execução de **Reminders** precisam implementar a interface `IRemindable`. O mais fácil é implementar na interface do **Grain** mesmo, sem mistério. Os **Grain** precisam ser marcados com esta interface para que o Orleans saiba que eles recebem notificações para a execução dos **Reminders**.

```csharp
public interface IExampleGrain : IGrainWithIntegerKey, IRemindable
{
	Task ActivateReminder();
	Task DeactivateGrain();
	Task DeactivateReminder();
}
```

Para registrar **ou** atualizar um **Reminder**, usamos o método **RegisterOrUpdateReminder**. Este método aceita três parâmetros

- O primeiro parâmetro é a identificação do **Reminder**. É esta identificação que o **Grain** recebe para saber qual **Reminder** será executado.
- O segundo parâmetro é o tempo que será passado para a primeira execução do **Reminder**. Neste caso, quando o `reminder1` será executado pela primeira vez cinco segundos depois do registro.
- O terceiro parâmetro é em quanto tempo o  **Reminder** será executado **após a primeira execução** - neste caso, será a cada minuto. Vale destacar que este é o tempo mínimo de intervalo de execução para **Reminders**.

```csharp
public async Task ActivateReminder()
{
	// tempo mínimo de registro entre execuções de um reminder é de 1 minuto
	await RegisterOrUpdateReminder("reminder1", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60));
}
```

Para desativar o **Reminder**, primeiramente obtenha a referência do mesmo com o método `GetReminder` e depois use o método `UnregisterReminder`. Vale destacar duas coisas aqui: a primeira é que a referência ao **Reminder** também devolvida no próprio método `RegisterOrUpdateReminder`, caso seja mais fácil. A segunda é que o  método `GetReminders()`  devolve uma lista com todos os **Reminders** cadastrados. Note que todos os métodos devolvem `Tasks` - portanto podem ser chamados num contexto assíncrono.

```csharp
public async Task DeactivateReminder()
{
	var reminder = await GetReminder("reminder1");
    if (reminder != null)
		await UnregisterReminder(reminder);
}
```

Por último, precisamos implementar o método `ReceiveReminder` (da interface `IRemindable`), pois este método que é disparado toda vez que um **Reminder** do **Grain** é disparado pelo Orleans - mesmo que mais de um **Reminder** esteja cadastrado! Recebemos o nome do **Reminder** (aquele nome usado para o registro/atualização) juntamente com informações sobre o horário de execução do mesmo, na variável do tipo `TickStatus`. 

Precisamos discriminar o que será executado de fato com base no nome do **Reminder**, como no exemplo abaixo. Fique esperto também com as informações de horário na variável do tipo `TickStatus`, pois os horários são cadastrados e recuperados em UTC!

```csharp
public async Task ReceiveReminder(string reminderName, TickStatus status)
{
	if (reminderName == "reminder1")
	{
		await Task.Factory.StartNew(() =>
		{
			_value++;
			Console.WriteLine($"{status.CurrentTickTime:HH:mm:ss.ffff} - New value is {_value}");
		});
	}
}
```

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 7. Reminders na prática

No **Client**, temos um pequeno roteiro que mostra na prática os comportamentos do **Reminder** no **Grain**. De forma esperada, quando chamamos o método `ActivateReminder` do **Grain**, o **Reminder** é ativado e começará a ser executado primeiramente em 5 segundos e depois a cada minuto (dadas as configurações do mesmo).

```csharp
 // aqui o reminder é ativado
await grain.ActivateReminder();
Console.WriteLine("Reminder ativado/atualizado...");
Console.ReadKey(true);
```

Se chamarmos o método novamente, o **Reminder** é atualizado, o que significa que ele volta a ser executado primeiramente em 5 segundos e depois a cada minuto (dadas as configurações do mesmo).

```csharp
// aqui o reminder é atualizado, pois já existe
await grain.ActivateReminder();
Console.WriteLine("Reminder ativado/atualizado...");
Console.ReadKey(true);
```

A desativação do **Reminder** também é simples. No caso, o método `DeactivateReminder` do **Grain** remove qualquer referência ao **Reminder**.

```csharp
// aqui o reminder será desativado
await grain.DeactivateReminder();
Console.WriteLine("Reminder desativado...");
Console.ReadKey(true);
```

O que pode ser um problema, porque se tentarmos desativar novamente o **Reminder** que já não existe mais, podemos ter uma exceção de referência nula. Por isso que fixemos a checagem no método `DeactivateReminder` do **Grain**.

```csharp
// aqui o reminder será desativado, mas nada acontece porque ele já foi desativado
await grain.DeactivateReminder();
Console.WriteLine("Reminder desativado...");
Console.ReadKey(true);
```

Por último, se ativarmos o **Reminder** e o **Grain** for desativado, o **Reminder** continua funcionando normalmente - e sim, mesmo com o **Client** não funcionando mais! O Orleans, sem precisar de intervenção, ativa o **Grain** em algum **Silo** e envia o sinal de execução do **Reminder**

```csharp
// aqui o reminder é ativado novamente
await grain.ActivateReminder();
Console.WriteLine("Reminder ativado/atualizado...");
Console.ReadKey();

// aqui o grain é desativado, mas o reminder continua,
// o que fará o grain ser ativado
// do lado do servidor
await grain.DeactivateGrain();
Console.WriteLine("Grain desativado, mas o reminder o reativará...");
Console.ReadKey(true);
```

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

# 8. Sumário

- **Reminders** precisam de persistência própria para funcionarem corretamente.
- **Reminders** podem ser criados/atualizados com a mesma referência.
- Uma vez que um **Reminder** foi cadastrado, não há necessidade de manter o **Grain** ativado, pois o Orleans já faz a ativação do mesmo (se necessário) para o processamento do **Reminder**.
- Como precisa-se de uma infraestrutura maior para a execução dos **Reminders**, há um prazo mínimo espera de 1 minuto para entre as execuções de um **Reminder**.

<div align="right">
	
[Voltar](#projeto-graintimers)

</div>

[docker-site]: https://www.docker.com/
[docker-shortcuts]: https://github.com/prrandrade/DockerShortcuts
[readme-parte2]: https://github.com/prrandrade/OrleansStudy/tree/master/Parte%202%20-%20Computa%C3%A7%C3%A3o%20distribu%C3%ADda%20e%20persist%C3%AAncia%20com%20o%20Orleans
