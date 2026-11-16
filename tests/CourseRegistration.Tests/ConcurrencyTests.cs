namespace CourseRegistration.Tests;

public sealed class ConcurrencyTests
{
    private const string CourseId = "CS101";

    [Fact]
    public async Task ConcurrentEnrollmentHonorsCapacityAndCreatesOneRecordPerLearner()
    {
        var learners = Enumerable.Range(0, 100).Select(i => new Learner($"learner-{i:D3}", true)).ToArray();
        var service = new RegistrationService(CourseId, 10, learners);

        var results = await RunTogether(learners.SelectMany(learner =>
            Enumerable.Range(0, 3).Select(_ => (Func<RegistrationResult>)(() =>
            {
                var result = service.Enroll(CourseId, learner.Id);
                AssertInvariants(service.GetSnapshot());
                return result;
            }))));

        Assert.Equal(10, results.Count(r => r.Status == RegistrationStatus.Enrolled));
        Assert.Equal(90, results.Count(r => r.Status == RegistrationStatus.Waitlisted));
        Assert.Equal(20, results.Count(r => r.Status == RegistrationStatus.AlreadyEnrolled));
        Assert.Equal(180, results.Count(r => r.Status == RegistrationStatus.AlreadyWaitlisted));
        var state = service.GetSnapshot();
        Assert.Equal(10, state.EnrolledLearnerIds.Count);
        Assert.Equal(90, state.WaitlistedLearnerIds.Count);
        AssertInvariants(state);

        // Capture the actual serialized arrival order; scheduler order is deliberately unspecified.
        var expectedPromotionOrder = state.WaitlistedLearnerIds.ToArray();
        var currentLearner = state.EnrolledLearnerIds[0];
        foreach (var expected in expectedPromotionOrder)
        {
            var drop = service.Drop(CourseId, currentLearner);
            Assert.Equal(expected, drop.PromotedLearnerId);
            currentLearner = expected;
            AssertInvariants(service.GetSnapshot());
        }
        Assert.Empty(service.GetSnapshot().WaitlistedLearnerIds);
    }

    [Fact]
    public async Task ConcurrentDuplicateDropsReleaseOnlyOneSeatAndPromoteOnce()
    {
        var service = new RegistrationService(CourseId, 1,
            [new("a", true), new("b", true), new("c", true)]);
        service.Enroll(CourseId, "a");
        service.Enroll(CourseId, "b");
        service.Enroll(CourseId, "c");

        var results = await RunTogether(Enumerable.Range(0, 100)
            .Select(_ => (Func<RegistrationResult>)(() => service.Drop(CourseId, "a"))));

        Assert.Single(results, r => r.Status == RegistrationStatus.Dropped);
        Assert.Equal(99, results.Count(r => r.Status == RegistrationStatus.NotRegistered));
        Assert.Single(results, r => r.PromotedLearnerId == "b");
        Assert.Equal(["b"], service.GetSnapshot().EnrolledLearnerIds);
        Assert.Equal(["c"], service.GetSnapshot().WaitlistedLearnerIds);
        AssertInvariants(service.GetSnapshot());
    }

    [Fact]
    public async Task ConcurrentDistinctDropsAndNewEnrollmentsPreserveExistingQueuePriority()
    {
        var learners = Enumerable.Range(0, 100).Select(i => new Learner($"learner-{i:D3}", true)).ToArray();
        var service = new RegistrationService(CourseId, 10, learners);
        foreach (var learner in learners.Take(40))
            service.Enroll(CourseId, learner.Id);

        var drops = learners.Take(10).Select(learner =>
            (Func<RegistrationResult>)(() => service.Drop(CourseId, learner.Id)));
        var enrollments = learners.Skip(40).Select(learner =>
            (Func<RegistrationResult>)(() => service.Enroll(CourseId, learner.Id)));

        var results = await RunTogether(drops.Concat(enrollments));

        Assert.Equal(10, results.Count(r => r.Status == RegistrationStatus.Dropped));
        Assert.Equal(60, results.Count(r => r.Status == RegistrationStatus.Waitlisted));
        var state = service.GetSnapshot();
        Assert.Equal(learners.Skip(10).Take(10).Select(l => l.Id), state.EnrolledLearnerIds);
        Assert.Equal(learners.Skip(20).Take(20).Select(l => l.Id), state.WaitlistedLearnerIds.Take(20));
        Assert.Equal(80, state.WaitlistedLearnerIds.Count);
        AssertInvariants(state);
    }

    private static async Task<RegistrationResult[]> RunTogether(IEnumerable<Func<RegistrationResult>> actions)
    {
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tasks = actions.Select(action => Task.Run(async () =>
        {
            await start.Task;
            return action();
        })).ToArray();
        start.SetResult();
        return await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(30));
    }

    private static void AssertInvariants(CourseSnapshot state)
    {
        Assert.InRange(state.EnrolledLearnerIds.Count, 0, state.Capacity);
        Assert.Equal(state.EnrolledLearnerIds.Count, state.EnrolledLearnerIds.Distinct().Count());
        Assert.Equal(state.WaitlistedLearnerIds.Count, state.WaitlistedLearnerIds.Distinct().Count());
        Assert.Empty(state.EnrolledLearnerIds.Intersect(state.WaitlistedLearnerIds));
    }
}
