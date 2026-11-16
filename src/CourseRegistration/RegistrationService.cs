namespace CourseRegistration;

/// <summary>Owns one offering's in-memory registration state. All operations are atomic.</summary>
public sealed class RegistrationService
{
    private readonly object _sync = new();
    private readonly string _courseId;
    private readonly int _capacity;
    private readonly Dictionary<string, bool> _prerequisites = new(StringComparer.Ordinal);
    private readonly HashSet<string> _enrolled = new(StringComparer.Ordinal);
    private readonly LinkedList<string> _waitlist = new();

    public RegistrationService(string courseId, int capacity, IEnumerable<Learner> learners)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentNullException.ThrowIfNull(learners);

        _courseId = courseId;
        _capacity = capacity;
        foreach (var learner in learners)
        {
            ArgumentNullException.ThrowIfNull(learner);
            ArgumentException.ThrowIfNullOrWhiteSpace(learner.Id);
            if (!_prerequisites.TryAdd(learner.Id, learner.PrerequisiteCompleted))
                throw new ArgumentException("Learner IDs must be unique.", nameof(learners));
        }
    }

    public RegistrationResult Enroll(string? courseId, string? learnerId)
    {
        lock (_sync)
        {
            var failure = ValidateRequest(courseId, learnerId);
            if (failure is not null)
                return failure;

            // A retry preserves membership and queue position even if eligibility has changed.
            if (_enrolled.Contains(learnerId!))
                return new(RegistrationStatus.AlreadyEnrolled);
            if (_waitlist.Contains(learnerId!))
                return new(RegistrationStatus.AlreadyWaitlisted);
            if (!_prerequisites[learnerId!])
                return new(RegistrationStatus.PrerequisiteNotMet);

            if (_enrolled.Count < _capacity)
            {
                _enrolled.Add(learnerId!);
                return new(RegistrationStatus.Enrolled);
            }

            _waitlist.AddLast(learnerId!);
            return new(RegistrationStatus.Waitlisted);
        }
    }

    public RegistrationResult Drop(string? courseId, string? learnerId)
    {
        lock (_sync)
        {
            var failure = ValidateRequest(courseId, learnerId);
            if (failure is not null)
                return failure;

            if (_enrolled.Remove(learnerId!))
                return new(RegistrationStatus.Dropped, PromoteNextEligibleLearner());

            return _waitlist.Remove(learnerId!)
                ? new(RegistrationStatus.WaitlistCancelled)
                : new(RegistrationStatus.NotRegistered);
        }
    }

    /// <summary>
    /// Updates the trusted prerequisite flag. Existing enrollment is retained; queued eligibility
    /// is rechecked at promotion. This is an in-process data update, not an identity/admin API.
    /// </summary>
    public RegistrationResult SetPrerequisiteCompleted(string? courseId, string? learnerId, bool completed)
    {
        lock (_sync)
        {
            var failure = ValidateRequest(courseId, learnerId);
            if (failure is not null)
                return failure;

            _prerequisites[learnerId!] = completed;
            return new(RegistrationStatus.PrerequisiteUpdated);
        }
    }

    public CourseSnapshot GetSnapshot()
    {
        lock (_sync)
        {
            return new(
                _courseId,
                _capacity,
                Array.AsReadOnly(_enrolled.Order(StringComparer.Ordinal).ToArray()),
                Array.AsReadOnly(_waitlist.ToArray()));
        }
    }

    // The caller holds _sync for the full validation/mutation transaction.
    private RegistrationResult? ValidateRequest(string? courseId, string? learnerId)
    {
        if (string.IsNullOrWhiteSpace(courseId))
            return new(RegistrationStatus.InvalidCourseId);
        if (!StringComparer.Ordinal.Equals(_courseId, courseId))
            return new(RegistrationStatus.CourseNotFound);
        if (string.IsNullOrWhiteSpace(learnerId))
            return new(RegistrationStatus.InvalidLearnerId);
        if (!_prerequisites.ContainsKey(learnerId))
            return new(RegistrationStatus.LearnerNotFound);
        return null;
    }

    private string? PromoteNextEligibleLearner()
    {
        while (_waitlist.First is { } first)
        {
            var learnerId = first.Value;
            _waitlist.RemoveFirst();
            if (!_prerequisites[learnerId])
                continue;

            _enrolled.Add(learnerId);
            return learnerId;
        }

        return null;
    }
}
