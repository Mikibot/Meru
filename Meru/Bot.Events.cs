﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Meru.Common;

namespace Meru
{
    public partial class Bot
    {
        public event Func<IMessageObject, Task> OnMessageReceived;
    }
}
