using Microsoft.Extensions.Configuration;

namespace DrehenBot.Config
{
    public class AppConfig
    {
        private IConfiguration _conf;

        public AppConfig(IConfiguration conf)
        {
            _conf = conf;
            Discord = _conf.GetSection("Discord").Get<Discord>() ?? new Discord();
            SeedGenerator = _conf.GetSection("SeedGenerator").Get<SeedGenerator>() ?? new SeedGenerator();

            DiscordRoles = new List<DiscordRole>();
            foreach(var role in _conf.GetSection("DiscordRoles").GetChildren())
            {
                DiscordRoles.Add(new DiscordRole(role.Key, long.Parse(role.Value ?? "0")));
            }
        }

        public Discord Discord { get; set; }

        public SeedGenerator SeedGenerator { get; set; }

        public IList<DiscordRole> DiscordRoles { get; set; }

        public IConfiguration Configuration => _conf;
    }
}
