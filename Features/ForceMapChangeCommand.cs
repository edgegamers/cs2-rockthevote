using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace cs2_rockthevote;

public partial class Plugin {
    [ConsoleCommand("changemap", "Changes map to specified map")]
    //[RequiresPermissions("@css/changemap")]
    public void OnForceChangeMap(CCSPlayerController? player, CommandInfo? command) {
        if (player == null) return;
        
        if (command == null || string.IsNullOrEmpty(command.ArgString)) {
            player.PrintToChat("You did not specify what map to change to.");
            return;
        }
        
        var map = command.GetArg(1).Trim().ToLower();
        _forceMapChangeManager.ForceChangeMap(player, map);
    }
}

public class ForceMapChangeCommand : IPluginDependency<Plugin, Config> {

    private readonly MapLister _mapLister;   
    private readonly StringLocalizer _localizer;
    
    public ForceMapChangeCommand(MapLister mapLister, StringLocalizer localizer)
    {
        _mapLister = mapLister;
        _localizer = localizer;
    }

    public void ForceChangeMap(CCSPlayerController? player, string map)
    {
        if (_mapLister.Maps!.Select(x => x.Name)
                .FirstOrDefault(x => x.ToLower() == map) is null) {
            var result = _mapLister.Maps!.Select(x => x.Name)
                .FirstOrDefault(x => x.ToLower().Contains(map));
            if (result == null) {
                player!.PrintToChat(
                    _localizer.LocalizeWithPrefix("general.invalid-map"));
                return;
            }

            map = result;
        }
    }
}