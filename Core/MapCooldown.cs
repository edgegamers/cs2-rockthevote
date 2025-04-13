using CounterStrikeSharp.API;

namespace cs2_rockthevote.Core;

public class MapCooldown : IPluginDependency<Plugin, Config> {
  private ushort InCoolDown;
  private readonly List<string> mapsOnCoolDown = new();

  public MapCooldown(MapLister mapLister) {
    //this is called on map start
    mapLister.EventMapsLoaded += (e, maps) => {
      var map = Server.MapName;
      if (map is not null) {
        if (InCoolDown == 0) {
          mapsOnCoolDown.Clear();
          return;
        }

        if (mapsOnCoolDown.Count > InCoolDown) mapsOnCoolDown.RemoveAt(0);

        mapsOnCoolDown.Add(map.Trim().ToLower());
        EventCooldownRefreshed?.Invoke(this, maps);
      }
    };
  }

  public void OnConfigParsed(Config config) {
    InCoolDown = config.MapsInCoolDown;
  }

  public event EventHandler<Map[]>? EventCooldownRefreshed;

  public bool IsMapInCooldown(string map) {
    return mapsOnCoolDown.IndexOf(map) > -1;
  }
}