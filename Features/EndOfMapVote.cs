using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using cs2_rockthevote.Core;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace cs2_rockthevote;

public class EndOfMapVote : IPluginDependency<Plugin, Config> {
  private EndOfMapConfig _config = new();
  private ConVar? _gameMode;
  private readonly GameRules _gameRules;
  private ConVar? _gameType;
  private readonly MaxRoundsManager _maxRounds;
  private readonly PluginState _pluginState;
  private readonly TimeLimitManager _timeLimit;
  private Timer? _timer;
  private readonly EndMapVoteManager _voteManager;

  public EndOfMapVote(TimeLimitManager timeLimit, MaxRoundsManager maxRounds,
    PluginState pluginState, GameRules gameRules,
    EndMapVoteManager voteManager) {
    _timeLimit   = timeLimit;
    _maxRounds   = maxRounds;
    _pluginState = pluginState;
    _gameRules   = gameRules;
    _voteManager = voteManager;
  }

  public void OnMapStart(string map) { KillTimer(); }


  public void OnLoad(Plugin plugin) {
    void MaybeStartTimer() {
      KillTimer();
      if (!_timeLimit.UnlimitedTime && _config.Enabled)
        _timer = plugin.AddTimer(1.0F, () => {
          if (_gameRules is not null && !_gameRules.WarmupRunning
            && !_pluginState.DisableCommands && _timeLimit.TimeRemaining > 0)
            if (CheckTimeLeft())
              StartVote();
        }, TimerFlags.REPEAT);
    }

    plugin.RegisterEventHandler<EventRoundStart>((ev, info) => {
      if (!_pluginState.DisableCommands && !_gameRules.WarmupRunning
        && CheckMaxRounds() && _config.Enabled)
        StartVote();
      else
        MaybeStartTimer();

      return HookResult.Continue;
    });

    plugin.RegisterEventHandler<EventRoundAnnounceMatchStart>((ev, info) => {
      MaybeStartTimer();
      return HookResult.Continue;
    });
  }

  public void OnConfigParsed(Config config) { _config = config.EndOfMapVote; }

  private bool CheckMaxRounds() {
    //Server.PrintToChatAll($"Remaining rounds {_maxRounds.RemainingRounds}, remaining wins: {_maxRounds.RemainingWins}, triggerBefore {_config.TriggerRoundsBeforEnd}");
    if (_maxRounds.UnlimitedRounds) return false;

    if (_maxRounds.RemainingRounds <= _config.TriggerRoundsBeforEnd)
      return true;

    return _maxRounds.CanClinch
      && _maxRounds.RemainingWins <= _config.TriggerRoundsBeforEnd;
  }


  private bool CheckTimeLeft() {
    return !_timeLimit.UnlimitedTime
      && _timeLimit.TimeRemaining <= _config.TriggerSecondsBeforeEnd;
  }

  public void StartVote() {
    KillTimer();
    if (_config.Enabled) _voteManager.StartVote(_config);
  }

  private void KillTimer() {
    _timer?.Kill();
    _timer = null;
  }
}