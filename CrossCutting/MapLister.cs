namespace cs2_rockthevote;

public class MapLister : IPluginDependency<Plugin, Config> {
  private Plugin? _plugin;

  public Map[]? Maps { get; private set; }
  public bool MapsLoaded { get; private set; }

  public void Clear() {
    MapsLoaded = false;
    Maps       = null;
  }

  public void OnMapStart(string _map) {
    if (_plugin is not null) LoadMaps();
  }


  public void OnLoad(Plugin plugin) {
    _plugin = plugin;
    LoadMaps();
  }

  public event EventHandler<Map[]>? EventMapsLoaded;

  private void LoadMaps() {
    Clear();
    var mapsFile = Path.Combine(_plugin!.ModulePath, "../maplist.txt");
    if (!File.Exists(mapsFile)) throw new FileNotFoundException(mapsFile);

    Maps = File.ReadAllText(mapsFile)
     .Replace("\r\n", "\n")
     .Split("\n")
     .Select(x => x.Trim())
     .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("//"))
     .Select(mapLine => {
        string[] args = mapLine.Split(":");
        return new Map(args[0], args.Length == 2 ? args[1] : null);
      })
     .ToArray();

    MapsLoaded = true;
    if (EventMapsLoaded is not null) EventMapsLoaded.Invoke(this, Maps!);
  }
}