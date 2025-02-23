using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace cs2_rockthevote;

public class GameRules : IPluginDependency<Plugin, Config> {
  private CCSGameRules? _gameRules;

  public float GameStartTime => _gameRules?.GameStartTime ?? 0;

  public bool WarmupRunning => _gameRules?.WarmupPeriod ?? false;

  public int TotalRoundsPlayed => _gameRules?.TotalRoundsPlayed ?? 0;

  public void OnLoad(Plugin plugin) {
    SetGameRulesAsync();
    plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
    plugin.RegisterEventHandler<EventRoundAnnounceWarmup>(OnAnnounceWarmup);
  }

  public void OnMapStart(string map) { SetGameRulesAsync(); }

  public void SetGameRules() {
    _gameRules = Utilities
     .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
     .FirstOrDefault()
    ?.GameRules;
  }

  public void SetGameRulesAsync() {
    _gameRules = null;
    new Timer(1.0F, () => { SetGameRules(); });
  }


  public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info) {
    SetGameRules();
    return HookResult.Continue;
  }

  public HookResult OnAnnounceWarmup(EventRoundAnnounceWarmup @event,
    GameEventInfo info) {
    SetGameRules();
    return HookResult.Continue;
  }
}