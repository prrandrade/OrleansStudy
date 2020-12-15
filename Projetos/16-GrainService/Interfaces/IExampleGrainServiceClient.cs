namespace Interfaces
{
    using Orleans.Services;

    public interface IExampleGrainServiceClient : IGrainServiceClient<IExampleGrainService>, IExampleGrainService { }
}
