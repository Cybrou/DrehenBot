using System.Diagnostics;
using Discord.Commands;
using DrehenBot.Config;
using Microsoft.Extensions.Logging;

namespace DrehenBot.Modules
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        private AppConfig Config { get; set; }

        private ILogger<CommandModule> Log { get; set; }

        public CommandModule(AppConfig config, ILogger<CommandModule> log)
        {
            Config = config;
            Log = log;
        }

        [Command("raceseed")]
        [Summary("Generate a race seed.")]
        public async Task RaceSeed()
        {
            await RaceSeed(Config.SeedGenerator.DefaultSettingString);
        }

        [Command("raceseed")]
        [Summary("Generate a race seed.")]
        public async Task RaceSeed(
            [Summary("An optional setting string")]
            string settingString
        )
        {
            if (string.IsNullOrEmpty(settingString))
            {
                settingString = Config.SeedGenerator.DefaultSettingString;
            }
            else
            {
                await ReplyAsync("Generating Seed...");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Config.SeedGenerator.GeneratorPath,
                    Arguments = $"generate2 idnull {settingString} true",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();

                    // Lire toutes les lignes de sortie
                    string[] outputLines = process.StandardOutput.ReadToEnd()
                                                                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    process.WaitForExit();

                    // Récupérer la dernière ligne contenant "SUCCESS:"
                    string? successLine = outputLines.LastOrDefault(line => line.StartsWith("SUCCESS:"));

                    if (!string.IsNullOrEmpty(successLine))
                    {
                        // Extraire la partie après "SUCCESS:"
                        string extractedValue = successLine.Substring(8).Trim(); // 8 = longueur de "SUCCESS:"
                        await ReplyAsync($"Seed generated. You can find it at : <{string.Format(Config.SeedGenerator.WebsiteUrlTemplate, extractedValue)}>");
                    }
                    else
                    {
                        await ReplyAsync("An error has occured");
                        Log.LogError("Error while generating a seed");
                        Log.LogError(string.Join(Environment.NewLine, outputLines));
                    }
                }
            }
        }
    }
}
