namespace Grains
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Orleans.Runtime.Services;

    public class ExampleGrainServiceClient : GrainServiceClient<IExampleGrainService>, IExampleGrainServiceClient
    {
        public ExampleGrainServiceClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // ponte do Grain para o GrainService
        public Task CallGrain(long i) => GrainService.CallGrain(i);
    }
}
