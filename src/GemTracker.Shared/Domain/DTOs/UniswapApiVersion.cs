﻿using System.ComponentModel;

namespace GemTracker.Shared.Domain.DTOs
{
    public enum UniswapApiVersion
    {
        [Description("v1")]
        V1 = 0,

        [Description("v2")]
        V2 = 1
    }
}