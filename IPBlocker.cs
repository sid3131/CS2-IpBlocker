using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace IPBlocker;

public class IPBlocker : BasePlugin
{
    public override string ModuleName => "IP Blocker";
    public override string ModuleAuthor => "Sid"; // I will not give credit to the guy who doesnt know how to use regex
    public override string ModuleVersion => "1.0.0";

    private Config _config;

    public override void Load(bool hotReload)
    {
        _config = LoadConfig();

        AddCommandListener("say", OnCommandSay);
        AddCommandListener("say_team", OnCommandSay);
    }

    private HookResult OnCommandSay(CCSPlayerController? player, CommandInfo commandinfo)
    {
        if (player == null) return HookResult.Continue;
        if (!IsIp(commandinfo.ArgString)) return HookResult.Continue;
        var name = player.PlayerName;

        Print(player, $"IP Posting is not allowed, {name}!");
        return HookResult.Handled;
    }

    private static void Print(CCSPlayerController player, string message) // cred Tian
    {
        string text = $"[IPBlocker]:{ChatColors.Red} {message}";
        player.PrintToChat(text);
    }


    private bool IsIp(string input)
    {
        const string ipPattern = @"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b";

        Match match = Regex.Match(input, ipPattern);

        if (!match.Success || Array.Exists(_config.AllowedIPs!, ip => ip == match.Value))
            return false;

        string[] parts = match.Value.Split('.');
        foreach (string part in parts)
        {
            if (int.TryParse(part, out int num))
            {
                if (num < 0 || num > 255)
                {
                    return false; // Not between IP range. 
                }
            }
            else
            {
                return false; // Invalid
            }
        }

        return true;
    }

    private Config LoadConfig()
    {
        var configPath = Path.Combine(ModuleDirectory, "allowedIPs.json");

        if (!File.Exists(configPath)) return CreateConfig(configPath);

        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

        return config;
    }

    private Config CreateConfig(string configPath)
    {
        var config = new Config
        {
            AllowedIPs = new[] { "1.1.1.1", "192.168.1.1" }
        };

        File.WriteAllText(configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("[IPBlocker] The configuration was successfully saved to a file: " + configPath);
        Console.ResetColor();

        return config;
    }
}

public class Config
{
    public string[]? AllowedIPs { get; set; }
}
