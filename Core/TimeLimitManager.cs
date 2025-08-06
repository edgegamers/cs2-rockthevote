using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace cs2_rockthevote.Core;

public class TimeLimitManager : IPluginDependency<Plugin, Config> {
  private readonly GameRules _gameRules;

  private ConVar? _timeLimit;

  public TimeLimitManager(GameRules gameRules) { _gameRules = gameRules; }

  private decimal TimeLimitValue
    => (decimal)(_timeLimit?.GetPrimitiveValue<float>() ?? 0F) * 60M;

  public bool UnlimitedTime => TimeLimitValue <= 0;

  public decimal TimePlayed {
    get {
      if (_gameRules.WarmupRunning) return 0;

      return (decimal)(Server.CurrentTime - _gameRules.GameStartTime);
    }
  }

  public decimal TimeRemaining {
    get {
      if (UnlimitedTime || TimePlayed > TimeLimitValue) return 0;

      return TimeLimitValue - TimePlayed;
    }
  }

  public void OnMapStart(string map) { LoadCvar(); }

  public void OnLoad(Plugin plugin) { LoadCvar(); }

  private void LoadCvar() { _timeLimit = ConVar.Find("mp_timelimit"); }

  public void AddTime(int seconds) {
  if (_timeLimit == null) return;
  var current = _timeLimit.GetPrimitiveValue<float>();
  _timeLimit.SetValue(current + (seconds / 60f));
  }

  public static void AddTimeRemaining(int time) {
    var gamerules = ServerExtensions.GetGameRules();
    if (gamerules == null) return;
    gamerules.RoundTime += time;

    var proxy = ServerExtensions.GetGameRulesProxy();
    if (proxy == null) return;
    Utilities.SetStateChanged(proxy, "CCSGameRulesProxy", "m_pGameRules");
  }
}