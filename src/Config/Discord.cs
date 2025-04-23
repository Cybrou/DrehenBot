namespace DrehenBot.Config
{
    public class Discord
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string BotToken { get; set; } = "";
        public ulong BotGuild { get; set; }
        public ulong BotChannel { get; set; }
    }
}
