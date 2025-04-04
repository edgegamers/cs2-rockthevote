using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace cs2_rockthevote;

public partial class Plugin {
  [ConsoleCommand("css_forcertv",
    "Forcefully triggers a rock the vote")]
  [RequiresPermissions("@css/changemap")]
  public void OnForceRTV(CCSPlayerController? player, CommandInfo? command) {
    _forceRTVManager.CommandHandler(player!);
  }
}

public class ForceRockTheVoteCommand : IPluginDependency<Plugin, Config> {
  private RtvConfig _config = new();
  private readonly EndMapVoteManager _endmapVoteManager;
  private readonly StringLocalizer _localizer;
  
  public ForceRockTheVoteCommand(EndMapVoteManager endmapVoteManager, StringLocalizer localizer) {
    _endmapVoteManager = endmapVoteManager;
    _localizer         = localizer;
  }

  public void OnConfigParsed(Config config) {
    _config      = config.Rtv;
  }

  public void CommandHandler(CCSPlayerController? player) {
    Server.PrintToChatAll(_localizer.LocalizeWithPrefix("forcertv.started", (player is null) ? "SERVER" : player.PlayerName));
    _endmapVoteManager.StartVote(_config);
  }
}