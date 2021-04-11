using ChatApp.Config;
using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace ChatApp.Domain
{
    public class Server : IServer
    {        
        private IChatAppConfig _config;
        private int _port;
        List<IWebSocketConnection> sockets = new List<IWebSocketConnection>();

        public Server(IChatAppConfig config, int port)
        {
            _config = config;
            _port = port;
            sockets = new List<IWebSocketConnection>();
        }

        public void Run()
        {
            try
            {
                StartServer();

                WaitForExitMessage();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void StartServer()
        {
            var server = new WebSocketServer($"ws://{_config.ServerIP}:{_port}");

            server.Start(socket =>
            {
                string nickName = GetNickName(socket);

                socket.OnOpen = () =>
                {
                    sockets.Add(socket);
                    InformAllUserJoined(nickName);
                };
                socket.OnClose = () =>
                {
                    sockets.Remove(socket);
                    InformAllUserDisconnected(nickName);
                };
                socket.OnMessage = message =>
                {
                    InformAllUserSentMessage(message, nickName);
                };
            });

            Console.WriteLine($"Listening on port {_port}");
        }

        private void WaitForExitMessage()
        {
            bool stop = false;
            while (!stop)
            {
                var input = Console.ReadLine();
                stop = IsExitMessage(input);
                if (stop)
                    Stop();
            }
        }
        public void Stop()
        {
            SendMessageToEveryone($"Server stopping on port {_port}...", sockets);
            SendMessageToEveryone(_config.ExitMessage, sockets);
            Console.WriteLine($"Server stopped on port {_port}");
        }

        private void InformAllUserSentMessage(string message, string nickName)
        {
            if (!IsExitMessage(message))
            {
                SendChatMessageToEveryone(message, sockets, nickName);
                Console.WriteLine($"{nickName}: {message}");
            }
        }

        private void InformAllUserDisconnected(string nickName)
        {
            var message = $"{nickName} has disconnected";
            SendMessageToEveryone(message, sockets);
            Console.WriteLine(message);
        }

        private void InformAllUserJoined(string nickName)
        {
            var message = $"{nickName} has joined";
            SendMessageToEveryone(message, sockets);
            Console.WriteLine(message);
        }        

        private static void SendChatMessageToEveryone(string message, List<IWebSocketConnection> sockets, string nickName)
        {
            sockets.ToList().ForEach(s => s.Send($"{nickName}: {message}"));
        }

        private static void SendMessageToEveryone(string message, List<IWebSocketConnection> sockets)
        {
            sockets.ToList().ForEach(s => s.Send(message));
        }

        private static string GetNickName(IWebSocketConnection socket)
        {
            return socket.ConnectionInfo.Path.Replace("/", "");
        }
       
        private bool IsExitMessage(string message)
        {
            return message == _config.ExitMessage;
        }

    }
}
