﻿namespace Interfaces
{
    using System.Threading.Tasks;
    using Orleans;

    public interface IHelloGrain : IGrainWithIntegerKey
    {
        Task Ping();
    }
}
