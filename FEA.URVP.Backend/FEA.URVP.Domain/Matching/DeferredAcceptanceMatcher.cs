namespace FEA.URVP.Domain.Matching;

/// <summary>A student's ranked preference for a project.</summary>
public sealed record StudentPreference(Guid StudentId, Guid ProjectId, byte Rank);

/// <summary>A faculty member's ranked preference for a candidate on their project.</summary>
public sealed record FacultyPreference(Guid ProjectId, Guid StudentId, byte Rank);

public sealed record MatchAssignment(
    Guid StudentId,
    Guid ProjectId,
    byte StudentRank,
    byte FacultyRank,
    bool ResolvedByTieBreak);

public sealed record MatchingOutcome(
    IReadOnlyList<MatchAssignment> Assignments,
    IReadOnlyList<Guid> UnmatchedStudentIds,
    int TieBreaksUsed);

/// <summary>
/// Student-proposing deferred acceptance (many-to-one Gale–Shapley).
/// Students propose in their preference order; each project tentatively holds
/// its best candidates up to capacity, ordered by faculty rank, then the
/// student's rank of the project, then a seeded lottery. Only mutually ranked
/// pairs are eligible: a candidate the faculty did not rank is never placed.
/// The result is stable and deterministic for a given seed.
/// </summary>
public static class DeferredAcceptanceMatcher
{
    public const string Version = "da-student-proposing/v1";

    public static MatchingOutcome Run(
        IReadOnlyDictionary<Guid, int> capacityByProject,
        IEnumerable<StudentPreference> studentPreferences,
        IEnumerable<FacultyPreference> facultyPreferences,
        int seed)
    {
        var facultyRank = facultyPreferences.ToDictionary(p => (p.ProjectId, p.StudentId), p => p.Rank);

        // Only proposals the faculty would accept and that target a project with seats.
        var preferences = studentPreferences
            .Where(p => capacityByProject.TryGetValue(p.ProjectId, out var cap) && cap > 0)
            .Where(p => facultyRank.ContainsKey((p.ProjectId, p.StudentId)))
            .GroupBy(p => p.StudentId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(p => p.Rank).ToList());

        var lottery = BuildLottery(preferences.Keys, seed);
        var nextChoice = preferences.Keys.ToDictionary(id => id, _ => 0);
        var held = new Dictionary<Guid, List<Candidate>>();
        var rejectedAt = new Dictionary<Guid, List<Candidate>>();
        var queue = new Queue<Guid>(preferences.Keys.OrderBy(id => lottery[id]));
        var unmatched = new List<Guid>();

        while (queue.Count > 0)
        {
            var studentId = queue.Dequeue();
            var choices = preferences[studentId];
            var index = nextChoice[studentId];

            if (index >= choices.Count)
            {
                unmatched.Add(studentId);
                continue;
            }

            var choice = choices[index];
            nextChoice[studentId] = index + 1;

            var candidate = new Candidate(
                studentId,
                choice.Rank,
                facultyRank[(choice.ProjectId, studentId)],
                lottery[studentId]);

            var holding = GetOrAdd(held, choice.ProjectId);
            holding.Add(candidate);

            if (holding.Count <= capacityByProject[choice.ProjectId])
            {
                continue;
            }

            holding.Sort(Candidate.Compare);
            var rejected = holding[^1];
            holding.RemoveAt(holding.Count - 1);
            GetOrAdd(rejectedAt, choice.ProjectId).Add(rejected);
            queue.Enqueue(rejected.StudentId);
        }

        var assignments = new List<MatchAssignment>();
        var tieBreaks = 0;

        foreach (var (projectId, candidates) in held)
        {
            var losers = rejectedAt.GetValueOrDefault(projectId) ?? [];

            foreach (var candidate in candidates)
            {
                var byLottery = losers.Any(l => l.TiesWith(candidate));
                if (byLottery) tieBreaks++;

                assignments.Add(new MatchAssignment(
                    candidate.StudentId,
                    projectId,
                    candidate.StudentRank,
                    candidate.FacultyRank,
                    byLottery));
            }
        }

        return new MatchingOutcome(assignments, unmatched, tieBreaks);
    }

    /// <summary>Deterministic per-student lottery numbers derived from the seed.</summary>
    private static Dictionary<Guid, int> BuildLottery(IEnumerable<Guid> studentIds, int seed)
    {
        var ordered = studentIds.OrderBy(id => id).ToList();
        var random = new Random(seed);
        var numbers = Enumerable.Range(0, ordered.Count).OrderBy(_ => random.Next()).ToList();
        return ordered.Select((id, i) => (id, numbers[i])).ToDictionary(x => x.id, x => x.Item2);
    }

    private static List<Candidate> GetOrAdd(Dictionary<Guid, List<Candidate>> map, Guid key)
    {
        if (!map.TryGetValue(key, out var list))
        {
            list = [];
            map[key] = list;
        }

        return list;
    }

    private sealed record Candidate(Guid StudentId, byte StudentRank, byte FacultyRank, int Lottery)
    {
        public bool TiesWith(Candidate other) =>
            FacultyRank == other.FacultyRank && StudentRank == other.StudentRank;

        public static int Compare(Candidate a, Candidate b)
        {
            var byFaculty = a.FacultyRank.CompareTo(b.FacultyRank);
            if (byFaculty != 0) return byFaculty;

            var byStudent = a.StudentRank.CompareTo(b.StudentRank);
            if (byStudent != 0) return byStudent;

            return a.Lottery.CompareTo(b.Lottery);
        }
    }
}
