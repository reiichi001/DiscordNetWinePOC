using Discord;

namespace DiscordNetWinePOC
{
    class Program
    {
        static void Main(string[] args)
            => new Program()
                .MainAsync()
                .GetAwaiter()
                .GetResult();

        public async Task MainAsync()
        {
            var Discord = new DiscordHandler();
            // Task t = this.Discord.Start(); // bot won't start if we just have this

            await Discord.Start();

            // Block the program until it is closed.
            await Task.Delay(Timeout.Infinite);
        }
    }
}