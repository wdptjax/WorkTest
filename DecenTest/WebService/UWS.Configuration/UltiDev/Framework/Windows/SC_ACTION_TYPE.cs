﻿namespace UltiDev.Framework.Windows
{
    using System;

    public enum SC_ACTION_TYPE : uint
    {
        SC_ACTION_NONE = 0,
        SC_ACTION_REBOOT = 2,
        SC_ACTION_RESTART = 1,
        SC_ACTION_RUN_COMMAND = 3
    }
}

