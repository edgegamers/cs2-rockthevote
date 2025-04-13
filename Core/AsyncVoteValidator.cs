namespace cs2_rockthevote;

public class AsyncVoteValidator {
  private readonly float VotePercentage;

  public AsyncVoteValidator(IVoteConfig config) {
    _config        = config;
    VotePercentage = _config.VotePercentage / 100F;
  }

  public int RequiredVotes
    => (int)Math.Round(ServerManager.ValidPlayerCount() * VotePercentage);

  private IVoteConfig _config { get; }

  public bool CheckVotes(int numberOfVotes) {
    return numberOfVotes > 0 && numberOfVotes >= RequiredVotes;
  }
}