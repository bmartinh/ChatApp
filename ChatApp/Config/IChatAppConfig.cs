using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Config
{
    public interface IChatAppConfig
    {
        string ServerIP { get; }
        string ExitMessage { get; }
    }
}
