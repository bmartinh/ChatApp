using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Config
{
    public class ChatAppConfig : IChatAppConfig
    {
        public string ServerIP { get { return "127.0.0.1"; } }
        public string ExitMessage { get { return "exit"; } }
    }
}
