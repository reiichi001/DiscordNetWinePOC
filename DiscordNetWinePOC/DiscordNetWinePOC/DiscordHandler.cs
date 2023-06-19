using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordNetWinePOC
{
    public class DiscordHandler : IDisposable
    {        
        private readonly DiscordSocketClient socketClient;

        public bool IsConnected => this.socketClient.ConnectionState == ConnectionState.Connected;
        public ulong UserId => this.socketClient.CurrentUser.Id;

        private string? DiscordToken = Environment.GetEnvironmentVariable("discordbottoken");
        private const string DiscordBotPrefix = "xl!";
        private const int EmbedColorFine = 0x478CFF;

        public DiscordHandler()
        {
            try
            {
                Console.WriteLine("BEFORE DiscordSocketClient");
                this.socketClient = new DiscordSocketClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 20, // hold onto the last 20 messages per channel in cache for duplicate checks
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMessages | GatewayIntents.GuildWebhooks | GatewayIntents.MessageContent,
                });
                Console.WriteLine("AFTER DiscordSocketClient");
                this.socketClient.Log += SocketClientOnLog;
                this.socketClient.Connected += SocketClientOnConnected;
                this.socketClient.Ready += SocketClientOnReady;
                this.socketClient.MessageReceived += SocketClientOnMessageReceived;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}\n\n{ex.Message}");
            }
            
        }

        public async Task Start()
        {

            if (string.IsNullOrEmpty(DiscordToken))
            {
                Console.WriteLine("Token empty, cannot start bot.");
                return;
            }

            try
            {
                await this.socketClient.LoginAsync(TokenType.Bot, DiscordToken);
                await this.socketClient.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}\nToken invalid, cannot start bot.");
            }

            Console.WriteLine("DiscordHandler START!!");
        }

        private Task SocketClientOnConnected()
        {
            Console.WriteLine("DiscordHandler CONNECTED!!");
            Console.WriteLine($"Connected as -> {socketClient.CurrentUser.Username}");
            return Task.CompletedTask;
        }
        private Task SocketClientOnLog(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
        private Task SocketClientOnReady()
        {
            Console.WriteLine("DiscordHandler READY!!");
            return Task.CompletedTask;
        }

        private async Task SocketClientOnMessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot || message.Author.IsWebhook)
                return;

            var args = message.Content.Split();

            // if it doesn't start with the bot prefix, ignore it.
            if (!args[0].StartsWith(DiscordBotPrefix))
                return;


            try
            {
                if (args[0] == DiscordBotPrefix + "ping")
                {
                    ISocketMessageChannel channel = message.Channel;
                    string replymessage = "pong";
                    string title = "Basic embed";
                    

                    await SendGenericEmbed(channel, replymessage, title, EmbedColorFine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}\nCould not handle incoming Discord message.");
            }
        }

        private async Task SendGenericEmbed(ISocketMessageChannel channel, string message, string title, uint color)
        {
            var builder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(message)
                .WithColor(new Color(color))
                .WithFooter(footer => {
                    footer
                        .WithText("Dalamud Chat Bridge");
                });
                
            var embed = builder.Build();
            await channel.SendMessageAsync(
                    null,
                    embed: embed)
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            Console.WriteLine("Discord DISPOSE!!");
            this.socketClient?.LogoutAsync().GetAwaiter().GetResult();
            this.socketClient?.Dispose();
        }
    }
}
