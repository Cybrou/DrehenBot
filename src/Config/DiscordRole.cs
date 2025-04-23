namespace DrehenBot.Config
{
    public class DiscordRole
    {
        public DiscordRole(string label, long discordId)
        {
            Label = label;
            DiscordId = discordId;
        }

        public string Label { get; set; }
        public long DiscordId { get; set; }
    }
}
