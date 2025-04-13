﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using cs2_rockthevote.Core;
using Microsoft.Extensions.Localization;

namespace cs2_rockthevote;

public partial class Plugin {
  [ConsoleCommand("votemap", "Vote to change to a map")]
  public void OnVotemap(CCSPlayerController? player, CommandInfo command) {
    var map = command.GetArg(1).Trim().ToLower();
    _votemapManager.CommandHandler(player!, map);
  }

  [GameEventHandler(HookMode.Pre)]
  public HookResult EventPlayerDisconnectVotemap(EventPlayerDisconnect @event,
    GameEventInfo _) {
    var player = @event.Userid;
    _votemapManager.PlayerDisconnected(player);
    return HookResult.Continue;
  }
}

public class VotemapCommand : IPluginDependency<Plugin, Config> {
  private readonly ChangeMapManager _changeMapManager;
  private VotemapConfig _config = new();
  private readonly GameRules _gamerules;
  private readonly StringLocalizer _localizer;
  private readonly MapCooldown _mapCooldown;
  private readonly MapLister _mapLister;
  private Plugin? _plugin;
  private readonly PluginState _pluginState;
  private readonly Dictionary<string, AsyncVoteManager> VotedMaps = new();
  private ChatMenu? votemapMenu;
  private CenterHtmlMenu? votemapMenuHud;

  public VotemapCommand(MapLister mapLister, GameRules gamerules,
    IStringLocalizer stringLocalizer, ChangeMapManager changeMapManager,
    PluginState pluginState, MapCooldown mapCooldown) {
    _mapLister = mapLister;
    _gamerules = gamerules;
    _localizer = new StringLocalizer(stringLocalizer, "votemap.prefix");
    _changeMapManager = changeMapManager;
    _pluginState = pluginState;
    _mapCooldown = mapCooldown;
    _mapCooldown.EventCooldownRefreshed += OnMapsLoaded;
  }

  public void OnMapStart(string map) { VotedMaps.Clear(); }

  public void OnConfigParsed(Config config) { _config = config.Votemap; }

  public void OnLoad(Plugin plugin) { _plugin = plugin; }

  public void OnMapsLoaded(object? sender, Map[] maps) {
    votemapMenu    = new ChatMenu("Votemap");
    votemapMenuHud = new CenterHtmlMenu("VoteMap");
    foreach (var map in _mapLister.Maps!.Where(x => x.Name != Server.MapName)) {
      votemapMenu.AddMenuOption(map.Name, (player, option) => {
        AddVote(player, option.Text);
        MenuManager.CloseActiveMenu(player);
      }, _mapCooldown.IsMapInCooldown(map.Name));

      votemapMenuHud.AddMenuOption(map.Name, (player, option) => {
        AddVote(player, option.Text);
        MenuManager.CloseActiveMenu(player);
      }, _mapCooldown.IsMapInCooldown(map.Name));
    }
  }

  public void CommandHandler(CCSPlayerController? player, string map) {
    if (player is null) return;

    map = map.ToLower().Trim();
    if (_pluginState.DisableCommands || !_config.Enabled) {
      player.PrintToChat(
        _localizer.LocalizeWithPrefix("general.validation.disabled"));
      return;
    }

    if (_gamerules.WarmupRunning) {
      if (!_config.EnabledInWarmup) {
        player.PrintToChat(
          _localizer.LocalizeWithPrefix("general.validation.warmup"));
        return;
      }
    } else if (_config.MinRounds > 0
      && _config.MinRounds > _gamerules.TotalRoundsPlayed) {
      player!.PrintToChat(_localizer.LocalizeWithPrefix(
        "general.validation.minimum-rounds", _config.MinRounds));
      return;
    }

    if (ServerManager.ValidPlayerCount() < _config!.MinPlayers) {
      player.PrintToChat(_localizer.LocalizeWithPrefix(
        "general.validation.minimum-players", _config!.MinPlayers));
      return;
    }

    if (string.IsNullOrEmpty(map))
      OpenVotemapMenu(player!);
    else
      AddVote(player, map);
  }

  public void OpenVotemapMenu(CCSPlayerController player) {
    if (_config.HudMenu)
      MenuManager.OpenCenterHtmlMenu(_plugin, player, votemapMenuHud!);
    else
      MenuManager.OpenChatMenu(player, votemapMenu!);
  }

  private void AddVote(CCSPlayerController player, string map) {
    if (map == Server.MapName) {
      player!.PrintToChat(
        _localizer.LocalizeWithPrefix("general.validation.current-map"));
      return;
    }

    if (_mapCooldown.IsMapInCooldown(map)) {
      player!.PrintToChat(
        _localizer.LocalizeWithPrefix(
          "general.validation.map-played-recently"));
      return;
    }

    if (_mapLister.Maps!.FirstOrDefault(x => x.Name.ToLower() == map) is null) {
      player!.PrintToChat(_localizer.LocalizeWithPrefix("general.invalid-map"));
      return;
    }

    var userId = player.UserId!.Value;
    if (!VotedMaps.ContainsKey(map))
      VotedMaps.Add(map, new AsyncVoteManager(_config));

    var voteManager = VotedMaps[map];
    var result      = voteManager.AddVote(userId);
    switch (result.Result) {
      case VoteResultEnum.Added:
        Server.PrintToChatAll(
          $"{_localizer.LocalizeWithPrefix("votemap.player-voted", player.PlayerName, map)} {_localizer.Localize("general.votes-needed", result.VoteCount, result.RequiredVotes)}");
        break;
      case VoteResultEnum.AlreadyAddedBefore:
        player.PrintToChat(
          $"{_localizer.LocalizeWithPrefix("votemap.already-voted", map)} {_localizer.Localize("general.votes-needed", result.VoteCount, result.RequiredVotes)}");
        break;
      case VoteResultEnum.VotesAlreadyReached:
        player.PrintToChat(_localizer.LocalizeWithPrefix("votemap.disabled"));
        break;
      case VoteResultEnum.VotesReached:
        Server.PrintToChatAll(
          $"{_localizer.LocalizeWithPrefix("votemap.player-voted", player.PlayerName, map)} {_localizer.Localize("general.votes-needed", result.VoteCount, result.RequiredVotes)}");
        _changeMapManager.ScheduleMapChange(map, prefix: "votemap.prefix");
        if (_config!.ChangeMapImmediatly)
          _changeMapManager.ChangeNextMap();
        else
          Server.PrintToChatAll(
            _localizer.LocalizeWithPrefix("general.changing-map-next-round",
              map));
        break;
    }
  }

  public void PlayerDisconnected(CCSPlayerController player) {
    var userId = player.UserId!.Value;
    foreach (var map in VotedMaps) map.Value.RemoveVote(userId);
  }
}