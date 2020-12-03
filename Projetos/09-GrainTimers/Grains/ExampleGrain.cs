namespace Grains
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans;

    public class ExampleModel
    {
        public int Value { get; set; }
    }

    public class ExampleGrain : Grain, IExampleGrain
    {
        private IDisposable _timer;
        private readonly ExampleModel _teste = new ExampleModel { Value = 0 };

        public Task ActivateTimer()
        {
            if (_timer == null)
            {
                _timer = RegisterTimer(obj =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        Console.WriteLine($"Timer called and the current value of {((ExampleModel)obj).Value}");
                        ((ExampleModel)obj).Value++;
                    });
                }, _teste, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2));
            }
            else
            {
                Console.WriteLine("Timer is already activated!");
            }

            return Task.CompletedTask;
        }

        public Task DeactivateGrain()
        {
            DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public Task DeactivateTimer()
        {
            if (_timer == null)
            {
                Console.WriteLine("Timer is already deactivated!");
            }
            else
            {
                _timer?.Dispose();
                _timer = null;
                Console.WriteLine("Timer deactivated.");
            }
            return Task.CompletedTask;
        }
    }
}
