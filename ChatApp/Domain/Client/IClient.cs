using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Domain.Client
{
    public interface IClient
    {
        void Run();
        void Stop();
    }
}
