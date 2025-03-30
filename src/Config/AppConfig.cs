using Microsoft.Extensions.Configuration;

namespace DrehenBot.Config
{
    internal class AppConfig
    {
        private IConfiguration _conf;

        public AppConfig(IConfiguration conf)
        {
            _conf = conf;
            Discord = _conf.GetSection("Discord").Get<Discord>() ?? new Discord();

            DiscordRoles = new List<DiscordRole>();
            foreach(var role in _conf.GetSection("DiscordRoles").GetChildren())
            {
                DiscordRoles.Add(new DiscordRole(role.Key, long.Parse(role.Value ?? "0")));
            }
        }

        public Discord Discord { get; set; }

        public IList<DiscordRole> DiscordRoles { get; set; }

        public IConfiguration Configuration => _conf;
    }
}
