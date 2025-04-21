﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Events;
using cs2_rockthevote.Features;
using Microsoft.Extensions.DependencyInjection;
using static CounterStrikeSharp.API.Core.Listeners;

namespace cs2_rockthevote;

public class PluginDependencyInjection : IPluginServiceCollection<Plugin> {
  public void ConfigureServices(IServiceCollection serviceCollection) {
    var di = new DependencyManager<Plugin, Config>();
    di.LoadDependencies(typeof(Plugin).Assembly);
    di.AddIt(serviceCollection);
    serviceCollection.AddScoped<StringLocalizer>();
  }
}

public partial class Plugin : BasePlugin, IPluginConfig<Config> {
  private readonly ChangeMapManager _changeMapManager;


  private readonly DependencyManager<Plugin, Config> _dependencyManager;
  private readonly NextMapCommand _nextMap;
  private readonly NominationCommand _nominationManager;
  private readonly RockTheVoteCommand _rtvManager;
  private readonly ForceMapChangeCommand _forceMapChangeManager;
  private readonly TimeLeftCommand _timeLeft;
  private readonly VotemapCommand _votemapManager;

  public Plugin(DependencyManager<Plugin, Config> dependencyManager,
    NominationCommand nominationManager, ChangeMapManager changeMapManager,
    VotemapCommand voteMapManager, RockTheVoteCommand rtvManager,
    TimeLeftCommand timeLeft, NextMapCommand nextMap, ForceMapChangeCommand forceMapChangeManager) {
    _dependencyManager = dependencyManager;
    _nominationManager = nominationManager;
    _changeMapManager  = changeMapManager;
    _votemapManager    = voteMapManager;
    _rtvManager        = rtvManager;
    _timeLeft          = timeLeft;
    _nextMap           = nextMap;
    
    _forceMapChangeManager = forceMapChangeManager;
  }

  public override string ModuleName => "RockTheVote";
  public override string ModuleVersion => "1.8.4";
  public override string ModuleAuthor => "abnerfs";

  public override string ModuleDescription
    => "https://github.com/abnerfs/cs2-rockthevote";

  public Config? Config { get; set; }

  public void OnConfigParsed(Config config) {
    Config = config;

    if (Config.Version < 9)
      Console.WriteLine(
        "[RockTheVote] please delete it from addons/counterstrikesharp/configs/plugins/RockTheVote and let the plugin recreate it on load");

    if (Config.Version < 7)
      throw new Exception(
        "Your config file is too old, please delete it from addons/counterstrikesharp/configs/plugins/RockTheVote and let the plugin recreate it on load");

    _dependencyManager.OnConfigParsed(config);
  }

  public string Localize(string prefix, string key, params object[] values) {
    return $"{Localizer[prefix]} {Localizer[key, values]}";
  }

  public override void Load(bool hotReload) {
    _dependencyManager.OnPluginLoad(this);
    RegisterListener<OnMapStart>(_dependencyManager.OnMapStart);
  }

  [GameEventHandler()]
  public HookResult OnChat(EventPlayerChat @event, GameEventInfo info) {
    var player = Utilities.GetPlayerFromUserid(@event.Userid);
    if (player is not null) {
      var text = @event.Text.Trim().ToLower();
      if (text == "rtv") { _rtvManager.CommandHandler(player); } else if (
        text.StartsWith("nominate")) {
        var split = text.Split("nominate");
        var map   = split.Length > 1 ? split[1].Trim() : "";
        _nominationManager.CommandHandler(player, map);
      } else if (text.StartsWith("votemap")) {
        var split = text.Split("votemap");
        var map   = split.Length > 1 ? split[1].Trim() : "";
        _votemapManager.CommandHandler(player, map);
      } else if (text.StartsWith("timeleft")) {
        _timeLeft.CommandHandler(player);
      } else if (text.StartsWith("nextmap")) {
        _nextMap.CommandHandler(player);
      }
    }

    return HookResult.Continue;
  }
}