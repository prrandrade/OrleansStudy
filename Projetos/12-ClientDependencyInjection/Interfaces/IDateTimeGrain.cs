﻿namespace Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Orleans;

    public interface IDateTimeGrain : IGrainWithIntegerKey
    {
        Task<DateTime> CurrentDateTime();
    }
}
