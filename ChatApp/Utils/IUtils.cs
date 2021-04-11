using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Utils
{
    public interface IUtils
    {
        bool IsPortBusy(int port);

        bool IsExitMessage(string message);
    }
}
