﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote.Features;

public class NextMapCommand : IPluginDependency<Plugin, Config> {
  private readonly ChangeMapManager _changeMapManager;
  private NextmapConfig _config = new();
  private readonly StringLocalizer _stringLocalizer;

  public NextMapCommand(ChangeMapManager changeMapManager,
    StringLocalizer stringLocalizer) {
    _changeMapManager = changeMapManager;
    _stringLocalizer  = stringLocalizer;
  }

  public void OnLoad(Plugin plugin) {
    plugin.AddCommand("nextmap", "Shows nextmap when defined",
      (player, info) => { CommandHandler(player); });
  }

  public void OnConfigParsed(Config config) {
    _config = config.Nextmap ?? new NextmapConfig();
  }

  public void CommandHandler(CCSPlayerController? player) {
    string text;
    if (_changeMapManager.NextMap is not null)
      text = _stringLocalizer.LocalizeWithPrefix("nextmap",
        _changeMapManager.NextMap);
    else
      text = _stringLocalizer.LocalizeWithPrefix("nextmap.decided-by-vote");

    if (_config.ShowToAll)
      Server.PrintToChatAll(text);
    else if (player is not null)
      player.PrintToChat(text);
    else
      Server.PrintToConsole(text);
  }
}