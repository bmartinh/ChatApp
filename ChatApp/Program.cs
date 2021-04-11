using ChatApp.Config;
using Fleck;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp
{
    [Command(Name = "ChatApp", Description = "Application that starts a chat server or connects to one depending on the port provided. If it's open, becomes a server, otherwise is a client. " +
        "Please provide username as second parameter for the client")]
    [HelpOption("-?")]
    class Program
    {
        static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0, Description = "Port to be used")]
        private int Port { get; }

        [Argument(1, Description = "The name of the user if it's a client")]
        private string Username { get; }

        private void OnExecute()
        {
            var serviceProvider = new ServiceCollection()                        
            .AddSingleton<IChatAppConfig, ChatAppConfig>()
            .BuildServiceProvider();

            try
            {  
                if (!IsPortBusy(Port))
                {
                    bool stop = false;
                    List<IWebSocketConnection> sockets = new List<IWebSocketConnection>();
                    var server = new WebSocketServer($"ws://127.0.0.1:{Port}");

                    server.Start(socket =>
                    {
                        string nickName = GetNickName(socket);

                        socket.OnOpen = () =>
                        {
                            sockets.Add(socket);
                            var message = $"{nickName} has joined";
                            SendMessageToEveryone(message, sockets);
                            Console.WriteLine(message);
                        };
                        socket.OnClose = () =>
                        {
                            sockets.Remove(socket);
                            var message = $"{nickName} has disconnected";
                            SendMessageToEveryone(message, sockets);
                            Console.WriteLine(message);                            
                        };
                        socket.OnMessage = message =>
                        {
                            if (!IsExitMessage(message))
                            {
                                SendChatMessageToEveryone(message, sockets, nickName);
                                Console.WriteLine($"{nickName}: {message}");
                            }                            
                        };
                    });

                    Console.WriteLine($"Listening on {Port}");

                    while (!stop)
                    {
                        var input = Console.ReadLine();
                        stop = IsExitMessage(input);                        
                    }
                }
                else
                {
                    RunClient();
                }                               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
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
      
        private bool IsPortBusy(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endPoints = ipGlobalProperties.GetActiveTcpListeners();            
            if (endPoints == null || endPoints.Length == 0) return false;

            foreach(IPEndPoint endPoint in endPoints)
            {
                if (endPoint.Port == port)
                    return true;
            }
           
            return false;
        }

        private void RunClient()
        {
            try
            {
                var webSocketClient = new ClientWebSocket();
                var tokenSource = new CancellationTokenSource();

                var task = webSocketClient.ConnectAsync(new Uri($"ws://127.0.0.1:{Port}/{Username}"), tokenSource.Token);
                task.Wait();
                task.Dispose();

                Console.WriteLine($"Logged in to chat in port {Port}");
                Console.WriteLine(@"Type ""exit"" to quit... ");
               
                Task readConsoleAndSend = Task.Run(() =>
                {
                    var input = "";
                    while(!IsExitMessage(input))
                    {
                        input = Console.ReadLine();

                        task = webSocketClient.SendAsync(
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(input)),
                                    WebSocketMessageType.Text,
                                    true,
                                    tokenSource.Token
                                );
                        task.Wait();
                        task.Dispose();
                    }
                    return;
                });

                Task readFromServer = Task.Run(() =>
                {
                    var input = "";
                    while (!IsExitMessage(input))
                    {
                        var buffer = new byte[1024];
                        var result = webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).GetAwaiter().GetResult();
                        input = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine(input);
                    }
                    return;
                });               

                var finishedTask = Task.WhenAny(new List<Task> { readConsoleAndSend , readFromServer }).GetAwaiter().GetResult();
              
                if (webSocketClient.State == WebSocketState.Open)
                {
                    task = webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", tokenSource.Token);
                    task.Wait();
                    task.Dispose();
                }

                tokenSource.Dispose();
                Console.WriteLine("WebSocket CLOSED");                              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }            
        }

        private bool IsExitMessage(string message)
        {
            return message == "exit";
        }

    }
}
