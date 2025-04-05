using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DrehenBot
{
    internal class Bot
    {
        private const string RoleMessageIdFileName = "roleMessageId";

        public Bot(Config.AppConfig appConfig, ILogger<Bot> log)
        {
            _appConfig = appConfig;
            _log = log;
        }

        private ILogger<Bot> _log;
        private Config.AppConfig _appConfig;
        private DiscordSocketClient _discord;
        private SocketGuild _guild;
        private SocketTextChannel _channel;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task<int> Start(CancellationToken? cancellationToken = null)
        {
            // Start Discord bot
            DiscordSocketConfig config = new DiscordSocketConfig();
            config.GatewayIntents = Discord.GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers;
            _discord = new DiscordSocketClient(config);
            _commands = new CommandService();
            _discord.Log += Discord_Log;

            _services = new ServiceCollection()
                .AddSingleton(_commands)
                .BuildServiceProvider();

            await _discord.LoginAsync(Discord.TokenType.Bot, _appConfig.Discord.BotToken);

            await RegisterCommandAsync();

            _discord.Ready += Discord_Ready;
            _discord.ButtonExecuted += Discord_ButtonExecuted;

            await _discord.StartAsync();
            if (cancellationToken != null)
            {
                await Task.Delay(-1).WaitAsync(cancellationToken.Value);
            }
            else
            {
                await Task.Delay(-1);
            }

            await _discord.StopAsync();
            await _discord.LogoutAsync();

            return 0;
        }

        public async Task RegisterCommandAsync()
        {
            _discord.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_discord, message);
            var channel = _discord.GetChannel(_appConfig.Discord.BotChannel) as SocketTextChannel;

            if (message.Author.IsBot) return;

            int argPos = 0;

            if (message.HasStringPrefix("!", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
                if(result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private Task Discord_Log(LogMessage log)
        {
            if (log.Exception != null)
            {
                _log.LogError(log.Exception, "Discord Client Error");
            }

            return Task.CompletedTask;
        }

        private async Task Discord_Ready()
        {
            _guild = _discord.GetGuild(_appConfig.Discord.BotGuild);
            _channel = _guild.GetTextChannel(_appConfig.Discord.BotChannel);

            await CreateOrUpdateRoleMessage();
        }

        private async Task CreateOrUpdateRoleMessage()
        {
            // Check if message already exists
            ulong? roleMessageId = await GetRoleMessageId();
            IMessage? roleMessage = null;
            if (roleMessageId != null)
            {
                roleMessage = await _channel.GetMessageAsync(roleMessageId.Value);
            }

            EmbedBuilder eb = new EmbedBuilder()
                .WithTitle("Roles management")
                .WithColor(new Color(0x1b, 0x75, 0xc4))
                .WithDescription("You can use the button to add or remove you from the Race role that will be pinged when a new race session is announced.");

            ComponentBuilder cb = new ComponentBuilder();
            foreach(var role in _appConfig.DiscordRoles)
            {
                cb.WithButton(role.Label, $"btn_role_{role.DiscordId}", ButtonStyle.Primary);
            }

            string? message = null;
            Embed embed = eb.Build();
            MessageComponent components = cb.Build();

            if (roleMessage == null)
            {
                RestUserMessage msg = await _channel.SendMessageAsync(message, embed: embed, components: components);
                await SaveRoleMessageId(msg.Id);
            }
            else
            {
                await _channel.ModifyMessageAsync(roleMessage.Id, msg =>
                {
                    msg.Content = message;
                    msg.Embed = embed;
                    msg.Components = components;
                });
            }
        }

        private async Task Discord_ButtonExecuted(SocketMessageComponent msg)
        {
            try
            {
                await msg.DeferAsync(true);
            }
            catch { }

            if (msg.Data.CustomId.StartsWith("btn_role_"))
            {
                string strRoleId = msg.Data.CustomId.Substring(9);
                ulong roleId;
                if (ulong.TryParse(strRoleId, out roleId))
                {
                    await ToggleUserRole(msg.User, roleId);
                }
            }
        }

        private async Task ToggleUserRole(SocketUser user, ulong roleId)
        {
            SocketGuildUser? guildUser = _guild.GetUser(user.Id);

            if (guildUser != null)
            {
                IRole? role = guildUser.Guild.Roles.FirstOrDefault(r => r.Id == roleId);
                if (role != null)
                {
                    if (guildUser.Roles.Contains(role))
                    {
                        await guildUser.RemoveRoleAsync(role);
                        await guildUser.SendMessageAsync($"You have been ungranted the {role.Name} role.");
                    }
                    else
                    {
                        await guildUser.AddRoleAsync(role);
                        await guildUser.SendMessageAsync($"You have been granted the {role.Name} role.");
                    }
                }
            }
        }


        private async Task<ulong?> GetRoleMessageId()
        {
            if (File.Exists(RoleMessageIdFileName))
            {
                string value = await File.ReadAllTextAsync(RoleMessageIdFileName);
                ulong id;
                if (!string.IsNullOrEmpty(value) && ulong.TryParse(value.Trim(), out id)) {
                    return id;
                }
            }

            return null;
        }

        private async Task SaveRoleMessageId(ulong messageId)
        {
            await File.WriteAllTextAsync(RoleMessageIdFileName, messageId.ToString());
        }
    }
}
