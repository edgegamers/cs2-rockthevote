using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace cs2_rockthevote;

public partial class Plugin {
    [ConsoleCommand("changemap", "Changes map to specified map")]
    //[RequiresPermissions("@css/changemap")]
    public void OnForceChangeMap(CommandInfo? command) {
        if (command == null) return;
        
        if (string.IsNullOrEmpty(command.ArgString)) {
            command.ReplyToCommand("You did not specify what map to change to.");
            return;
        }
        
        var map = command.GetArg(1).Trim().ToLower();
        _forceMapChangeManager.ForceChangeMap(command, map);
    }
}

public class ForceMapChangeCommand : IPluginDependency<Plugin, Config> {

    private readonly MapLister _mapLister;   
    private readonly ChangeMapManager _changeMapManager;
    private readonly StringLocalizer _localizer;
    
    public ForceMapChangeCommand(MapLister mapLister, ChangeMapManager changeMapManager, StringLocalizer localizer)
    {
        _mapLister = mapLister;
        _changeMapManager = changeMapManager;   
        _localizer = localizer;
    }

    public void ForceChangeMap(CommandInfo command, string map)
    {
        var maps = _mapLister.Maps;
        if (maps == null || maps.Length == 0) return;
        
        if (maps!.Select(x => x.Name)
                .FirstOrDefault(x => x.ToLower() == map) is null) {
            var result = maps!.Select(x => x.Name)
                .FirstOrDefault(x => x.ToLower().Contains(map));
            if (result == null) {
                command!.ReplyToCommand(
                    _localizer.LocalizeWithPrefix("general.invalid-map"));
                return;
            }

            map = result;
            
            _changeMapManager.ScheduleMapChange(map, true);
            _changeMapManager.ChangeNextMap(true);
        }
    }
}