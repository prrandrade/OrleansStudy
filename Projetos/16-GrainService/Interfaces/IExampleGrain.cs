﻿namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IExampleGrain : IGrainWithIntegerKey
    {
        Task CallGrainService();
    }
}
