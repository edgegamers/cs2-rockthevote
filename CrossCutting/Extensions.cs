using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace cs2_rockthevote;

public static class Extensions {
  public static bool
    ReallyValid(this CCSPlayerController? player, bool considerBots = false) {
    return player is not null && player.IsValid
      && player.Connected == PlayerConnectedState.PlayerConnected
      && (considerBots || !player.IsBot && !player.IsHLTV);
  }
}

public static class ServerExtensions {
  /// <summary>
  ///   Get the current CCSGameRules for the server
  /// </summary>
  /// <returns></returns>
  public static CCSGameRules? GetGameRules() {
    //	From killstr3ak
    return Utilities
     .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
     .First()
     .GameRules;
  }

  public static CCSGameRulesProxy? GetGameRulesProxy() {
    //	From killstr3ak
    return Utilities
     .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
     .FirstOrDefault();
  }
}