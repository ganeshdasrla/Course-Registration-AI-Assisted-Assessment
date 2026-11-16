namespace CourseRegistration;

/// <summary>A detached view. Waitlist IDs are in FIFO order; enrolled IDs are sorted ordinally.</summary>
public sealed record CourseSnapshot(
    string CourseId,
    int Capacity,
    IReadOnlyList<string> EnrolledLearnerIds,
    IReadOnlyList<string> WaitlistedLearnerIds);
