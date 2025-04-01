using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrehenBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task Ping()
        {
            return ReplyAsync("Pong!");
        }

        [Command("raceseed")]
        public async Task RaceSeed(params String[] stringArray)
        {
            const string URL = @"http://127.0.0.1:3500/s/";
            if (stringArray.Length == 0)
            {
                ReplyAsync("Please provide a seed.");
            }
            else
            {
                var folder = Directory.GetCurrentDirectory();
                var rando = "\\TP\\Rando\\Generator\\bin\\Debug\\net8.0";
                await ReplyAsync("Generating Seed...");
                //Process p = Process.Start(folder + rando + "\\TPRandomizer.exe", "generate2 idnull " + $"{stringArray[0]}" + " true");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = folder + rando + @"\\TPRandomizer.exe",
                    Arguments = "generate2 idnull " + $"{stringArray[0]}" + " true",
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
                    string successLine = outputLines.LastOrDefault(line => line.StartsWith("SUCCESS:"));

                    if (!string.IsNullOrEmpty(successLine))
                    {
                        // Extraire la partie après "SUCCESS:"
                        string extractedValue = successLine.Substring(8).Trim(); // 8 = longueur de "SUCCESS:"
                        await ReplyAsync("Seed generated. You can find it at :" + URL + extractedValue);
                    }
                    else
                    {
                        await ReplyAsync("An error has occured");
                    }
                }
                //await ReplyAsync("Seed generated. You can find it at " + URL);
            }
        }
    }
}
