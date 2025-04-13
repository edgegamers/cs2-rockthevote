﻿namespace cs2_rockthevote;

public record VoteResult(VoteResultEnum Result, int VoteCount,
  int RequiredVotes);

public class AsyncVoteManager {
  private readonly AsyncVoteValidator _voteValidator;
  private readonly List<int> votes = new();

  public AsyncVoteManager(IVoteConfig config) {
    _voteValidator = new AsyncVoteValidator(config);
  }

  public int VoteCount => votes.Count;
  public int RequiredVotes => _voteValidator.RequiredVotes;

  public bool VotesAlreadyReached { get; set; }

  public void OnMapStart(string _mapName) {
    votes.Clear();
    VotesAlreadyReached = false;
  }

  public VoteResult AddVote(int userId) {
    if (VotesAlreadyReached)
      return new VoteResult(VoteResultEnum.VotesAlreadyReached, VoteCount,
        RequiredVotes);

    VoteResultEnum? result = null;
    if (votes.IndexOf(userId) != -1) {
      result = VoteResultEnum.AlreadyAddedBefore;
    } else {
      votes.Add(userId);
      result = VoteResultEnum.Added;
    }

    if (_voteValidator.CheckVotes(votes.Count)) {
      VotesAlreadyReached = true;
      return new VoteResult(VoteResultEnum.VotesReached, VoteCount,
        RequiredVotes);
    }

    return new VoteResult(result.Value, VoteCount, RequiredVotes);
  }

  public void RemoveVote(int userId) {
    var index = votes.IndexOf(userId);
    if (index > -1) votes.RemoveAt(index);
  }
}