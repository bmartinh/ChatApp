using ChatApp.Config;
using ChatApp.Domain.Server;
using ChatApp.Domain.Client;
using ChatApp.Utils;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ChatApp
{
    [Command(Name = "ChatApp", Description = "Application that starts a chat server or connects to one depending on the port provided. " +
                                             "If it's open, becomes a server, otherwise is a client.")]
    [HelpOption("-?")]
    class Program
    {
        static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0, Description = "Port to be used")]
        private int Port { get; }

        [Argument(1, Description = "Chat nickname (only for client)")]
        private string Username { get; }

        private void OnExecute()
        {
            ServiceProvider serviceProvider = RegisterServices();

            try
            {
                var utils = serviceProvider.GetService<IUtils>();
                var config = serviceProvider.GetService<IChatAppConfig>();

                if (!utils.IsPortBusy(Port))
                {
                    var server = new Server(config, utils, Port);
                    server.Run();
                }
                else
                {
                    var client = new Client(config, utils, Port, Username);
                    client.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static ServiceProvider RegisterServices()
        {
            return new ServiceCollection()
                        .AddSingleton<IChatAppConfig, ChatAppConfig>()
                        .AddSingleton<IUtils, UtilsImpl>()
                        .BuildServiceProvider();
        }
    }
}
