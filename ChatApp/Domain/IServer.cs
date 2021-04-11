using Fleck;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Domain
{
    public interface IServer
    {        
        void Run();
        void Stop();
    }
}
