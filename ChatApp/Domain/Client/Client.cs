using ChatApp.Config;
using ChatApp.Utils;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Domain.Client
{
    public class Client : IClient
    {
        private IChatAppConfig _config;
        private IUtils _utils;
        private int _port;
        private string _userName;
        private ClientWebSocket _webSocketClient;
        private CancellationTokenSource _cancellationTokenSource;

        public Client(IChatAppConfig config, IUtils utils, int port, string userName)
        {
            _config = config;
            _utils = utils;
            _port = port;
            _userName = userName;
            _webSocketClient = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Run()
        {
            try
            {
                Login();

                Task readConsoleAndSend = ReadConsoleAndSendHandler();

                Task readFromServer = ReceiveFromServerHandler();

                Task.WhenAny(new List<Task> { readConsoleAndSend, readFromServer })
                    .GetAwaiter().GetResult();

                Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void Login()
        {
            if (string.IsNullOrEmpty(_userName))
                throw new ArgumentNullException("Username", "Username cannot be empty. Usage: ChatApp [Port] [Username]");

            _webSocketClient.ConnectAsync(new Uri($"ws://127.0.0.1:{_port}/{_userName}"), _cancellationTokenSource.Token)
                .GetAwaiter().GetResult();

            Console.WriteLine($"Logged in to chat in port {_port} as {_userName}");
            Console.WriteLine(@"Type ""exit"" to quit... ");
        }

        public void Stop()
        {
            if (_webSocketClient.State == WebSocketState.Open)
            {
                CloseConnection(_webSocketClient, _cancellationTokenSource, WebSocketCloseStatus.NormalClosure);

            }

            _cancellationTokenSource.Dispose();
            Console.WriteLine("Chat terminated");
        }

        private Task ReceiveFromServerHandler()
        {
            return Task.Run(() =>
            {
                var input = "";
                bool connectionClosed = false;
                while (!connectionClosed)
                {
                    var buffer = new byte[1024];
                    var result = _webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None)
                    .GetAwaiter().GetResult();

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        input = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine(input);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        CloseConnection(_webSocketClient, _cancellationTokenSource, result.CloseStatus.Value);
                        Console.WriteLine("Connection closed by the server");
                        connectionClosed = true;
                    }
                }
                return;
            });
        }

        private Task ReadConsoleAndSendHandler()
        {
            return Task.Run(() =>
            {
                var input = "";
                while (!_utils.IsExitMessage(input))
                {
                    input = Console.ReadLine();
                    SendMessage(input);
                }
                return;
            });
        }

        private void SendMessage(string input)
        {
            _webSocketClient.SendAsync(
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(input)),
                                    WebSocketMessageType.Text,
                                    true,
                                    _cancellationTokenSource.Token
                                ).GetAwaiter().GetResult();
        }

        private static void CloseConnection(ClientWebSocket webSocketClient, CancellationTokenSource tokenSource, WebSocketCloseStatus closeStatus)
        {
            webSocketClient.CloseAsync(closeStatus, "", tokenSource.Token).GetAwaiter().GetResult();
        }
    }
}
