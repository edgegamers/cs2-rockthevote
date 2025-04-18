using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace cs2_rockthevote;

public partial class Plugin {
    [RequiresPermissionsOr("@css/changemap", "@css/rcon")]
    [ConsoleCommand("changemap", "Changes map to specified map")]
    public void OnForceChangeMap(CCSPlayerController? player, CommandInfo? command) {
        if (command == null) return;
        
        if (string.IsNullOrEmpty(command.ArgString)) {
            command.ReplyToCommand("You did not specify what map to change to.");
            return;
        }
        
        var map = command.GetArg(1).Trim().ToLower();
        
        ForceMapChangeResult result = _forceMapChangeManager.ForceChangeMap(map);
        
        if (!result.Success) command!.ReplyToCommand(result.ErrorMessage);
    }
}

public class ForceMapChangeCommand : IPluginDependency<Plugin, Config> {

    private readonly MapLister _mapLister;   
    private readonly ChangeMapManager _changeMapManager;
    
    public ForceMapChangeCommand(MapLister mapLister, ChangeMapManager changeMapManager)
    {
        _mapLister = mapLister;
        _changeMapManager = changeMapManager;   
    }

    public ForceMapChangeResult ForceChangeMap(string map)
    {
        ForceMapChangeResult mapChangeResult = new ForceMapChangeResult();
        
        var maps = _mapLister.Maps;
        if (maps == null || maps.Length == 0) {
            mapChangeResult.FailedMapChange(ForceMapChangeResult.EmptyMapList);
            return mapChangeResult;
        }
        
        if (maps!.Select(x => x.Name)
                .FirstOrDefault(x => x.ToLower() == map) is null) {
            var result = maps!.Select(x => x.Name)
                .FirstOrDefault(x => x.ToLower().Contains(map));
            if (result == null)
            {
                mapChangeResult.FailedMapChange(ForceMapChangeResult.InvalidMapArg);
                return mapChangeResult; 
            }

            map = result;
            
            _changeMapManager.ScheduleMapChange(map, true);
            _changeMapManager.ChangeNextMap(true);
        }
        
        return mapChangeResult;
    }
}

public struct ForceMapChangeResult
{
    public ForceMapChangeResult() { }
    
    public const string EmptyMapList = "Map list was null or empty.";
    public const string InvalidMapArg = "Invalid map argument.";

    public bool Success { get; private set; } = true;
    
    public string ErrorMessage { get; private set; } = String.Empty;

    public void FailedMapChange(string failMessage)
    {
        Success = false;
        ErrorMessage = failMessage;
    }
}