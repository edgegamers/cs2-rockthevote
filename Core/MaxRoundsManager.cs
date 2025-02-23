using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace cs2_rockthevote;

public class MaxRoundsManager : IPluginDependency<Plugin, Config> {
  private ConVar? _canClinch;

  private readonly GameRules _gameRules;
  private bool _lastBeforeHalf;
  private ConVar? _maxRounds;
  private int CTWins;
  private int TWins;

  public MaxRoundsManager(GameRules gameRules) { _gameRules = gameRules; }

  private int MaxRoundsValue => _maxRounds?.GetPrimitiveValue<int>() ?? 0;
  public bool CanClinch => _canClinch?.GetPrimitiveValue<bool>() ?? true;

  public bool UnlimitedRounds => MaxRoundsValue <= 0;

  public int RemainingRounds {
    get {
      var played = MaxRoundsValue - _gameRules.TotalRoundsPlayed;
      if (played < 0) return 0;

      return played;
    }
  }

  public int RemainingWins => MaxWins - CurrentHighestWins;

  public int MaxWins {
    get {
      if (MaxRoundsValue <= 0) return 0;

      if (!CanClinch) return MaxRoundsValue;

      return (int)Math.Floor(MaxRoundsValue / 2M) + 1;
    }
  }

  public int CurrentHighestWins => CTWins > TWins ? CTWins : TWins;

  public void OnMapStart(string map) {
    LoadCvar();
    ClearRounds();
  }

  public void OnLoad(Plugin plugin) {
    plugin.RegisterEventHandler<EventRoundEnd>((@event, info) => {
      if (@event is null) return HookResult.Continue;

      CsTeam? winner = Enum.IsDefined(typeof(CsTeam), (byte)@event.Winner) ?
        (CsTeam)@event.Winner :
        null;
      if (winner is not null) RoundWin(winner.Value);

      if (_lastBeforeHalf) SwapScores();

      _lastBeforeHalf = false;
      return HookResult.Continue;
    });


    plugin.RegisterEventHandler<EventRoundAnnounceLastRoundHalf>((@event, info)
      => {
      if (@event is null) return HookResult.Continue;

      _lastBeforeHalf = true;
      return HookResult.Continue;
    });


    plugin.RegisterEventHandler<EventRoundAnnounceMatchStart>((@event, info)
      => {
      if (@event is null) return HookResult.Continue;

      ClearRounds();
      return HookResult.Continue;
    });

    LoadCvar();
    ClearRounds();
  }

  private void LoadCvar() {
    _maxRounds = ConVar.Find("mp_maxrounds");
    _canClinch = ConVar.Find("mp_match_can_clinch");
  }


  public void ClearRounds() {
    CTWins          = 0;
    TWins           = 0;
    _lastBeforeHalf = false;
  }

  private void SwapScores() {
    var oldCtWins = CTWins;
    CTWins = TWins;
    TWins  = oldCtWins;
  }

  public void RoundWin(CsTeam team) {
    if (team == CsTeam.CounterTerrorist)
      CTWins++;
    else if (team == CsTeam.Terrorist) TWins++;
    //Server.PrintToChatAll($"T Wins {TWins}, CTWins {CTWins}");
  }
}