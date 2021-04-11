using Fleck;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Domain.Server
{
    public interface IServer
    {
        void Run();
        void Stop();
    }
}
