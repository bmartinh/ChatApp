using ChatApp.Config;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ChatApp.Utils
{
    public class UtilsImpl : IUtils
    {
        private IChatAppConfig _config;
        public UtilsImpl(IChatAppConfig config)
        {
            _config = config;
        }

        bool IUtils.IsExitMessage(string message)
        {
            return message == _config.ExitMessage;
        }

        bool IUtils.IsPortBusy(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endPoints = ipGlobalProperties.GetActiveTcpListeners();
            if (endPoints == null || endPoints.Length == 0) return false;

            foreach (IPEndPoint endPoint in endPoints)
            {
                if (endPoint.Port == port)
                    return true;
            }

            return false;
        }
    }
}
