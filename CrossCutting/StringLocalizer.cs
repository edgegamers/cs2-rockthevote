using Microsoft.Extensions.Localization;

namespace cs2_rockthevote;

public class StringLocalizer {
  private readonly string _prefix;
  private readonly IStringLocalizer _localizer;

  public StringLocalizer(IStringLocalizer localizer) {
    _localizer = localizer;
    _prefix    = "rtv.prefix";
  }

  public StringLocalizer(IStringLocalizer localizer, string prefix) {
    _localizer = localizer;
    _prefix    = prefix;
  }

  public string LocalizeWithPrefixInternal(string prefix, string key,
    params object[] args) {
    return $"{_localizer[prefix]} {Localize(key, args)}";
  }

  public string LocalizeWithPrefix(string key, params object[] args) {
    return LocalizeWithPrefixInternal(_prefix, key, args);
  }

  public string Localize(string key, params object[] args) {
    return _localizer[key, args];
  }
}