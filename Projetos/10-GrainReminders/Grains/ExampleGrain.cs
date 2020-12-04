namespace Grains
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans;
    using Orleans.Runtime;

    public class ExampleGrain : Grain, IExampleGrain
    {
        private int _value;

        public async Task ActivateReminder()
        {
            // tempo mínimo de registro entre execuções de um reminder é de 1 minuto
            await RegisterOrUpdateReminder("reminder1", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60));
        }

        public Task DeactivateGrain()
        {
            DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public async Task DeactivateReminder()
        {
            var reminder = await GetReminder("reminder1");
            if (reminder != null)
                await UnregisterReminder(reminder);
        }

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
    }
}
