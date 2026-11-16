namespace CourseRegistration.Tests;

public sealed class RegistrationServiceTests
{
    private const string CourseId = "CS101";

    [Fact]
    public void EligibleLearnerEnrollsWhenSeatIsAvailable()
    {
        var service = Create(capacity: 2);

        Assert.Equal(new(RegistrationStatus.Enrolled), service.Enroll(CourseId, "a"));
        AssertState(service, ["a"], []);
    }

    [Fact]
    public void FullCourseWaitlistsInArrivalOrder()
    {
        var service = Create();
        service.Enroll(CourseId, "a");

        Assert.Equal(RegistrationStatus.Waitlisted, service.Enroll(CourseId, "c").Status);
        Assert.Equal(RegistrationStatus.Waitlisted, service.Enroll(CourseId, "b").Status);
        AssertState(service, ["a"], ["c", "b"]);
    }

    [Fact]
    public void DuplicateEnrollmentDoesNotConsumeAnotherSeat()
    {
        var service = Create(capacity: 2);
        service.Enroll(CourseId, "a");

        Assert.Equal(RegistrationStatus.AlreadyEnrolled, service.Enroll(CourseId, "a").Status);
        Assert.Equal(RegistrationStatus.Enrolled, service.Enroll(CourseId, "b").Status);
        AssertState(service, ["a", "b"], []);
    }

    [Fact]
    public void DuplicateWaitlistRequestDoesNotMoveLearnerToBack()
    {
        var service = FullWithWaitlist();

        Assert.Equal(RegistrationStatus.AlreadyWaitlisted, service.Enroll(CourseId, "b").Status);
        AssertState(service, ["a"], ["b", "c"]);
    }

    [Fact]
    public void DropPromotesExactlyOneLearnerInFifoOrder()
    {
        var service = FullWithWaitlist();

        Assert.Equal(new(RegistrationStatus.Dropped, "b"), service.Drop(CourseId, "a"));
        AssertState(service, ["b"], ["c"]);
        Assert.Equal(new(RegistrationStatus.Dropped, "c"), service.Drop(CourseId, "b"));
        AssertState(service, ["c"], []);
    }

    [Fact]
    public void NewcomerCannotTakeSeatReleasedToExistingWaitlist()
    {
        var service = FullWithWaitlist();
        service.Drop(CourseId, "a");

        Assert.Equal(RegistrationStatus.Waitlisted, service.Enroll(CourseId, "d").Status);
        AssertState(service, ["b"], ["c", "d"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void IneligibleLearnerCannotEnrollOrJoinWaitlist(int capacity)
    {
        var service = Create(capacity);

        Assert.Equal(RegistrationStatus.PrerequisiteNotMet, service.Enroll(CourseId, "ineligible").Status);
        AssertState(service, [], []);
    }

    [Fact]
    public void PromotionRemovesIneligibleHeadAndSelectsNextEligibleLearner()
    {
        var service = FullWithWaitlist();
        Assert.Equal(RegistrationStatus.PrerequisiteUpdated,
            service.SetPrerequisiteCompleted(CourseId, "b", false).Status);

        Assert.Equal(new(RegistrationStatus.Dropped, "c"), service.Drop(CourseId, "a"));
        AssertState(service, ["c"], []);
        Assert.Equal(RegistrationStatus.PrerequisiteNotMet, service.Enroll(CourseId, "b").Status);
    }

    [Fact]
    public void PromotionRemovesAllIneligibleEntriesAndLeavesSeatFree()
    {
        var service = FullWithWaitlist();
        service.SetPrerequisiteCompleted(CourseId, "b", false);
        service.SetPrerequisiteCompleted(CourseId, "c", false);

        Assert.Equal(new(RegistrationStatus.Dropped), service.Drop(CourseId, "a"));
        AssertState(service, [], []);
        Assert.Equal(RegistrationStatus.Enrolled, service.Enroll(CourseId, "d").Status);
    }

    [Fact]
    public void EligibilityRestoredBeforePromotionPreservesQueuePosition()
    {
        var service = FullWithWaitlist();
        service.SetPrerequisiteCompleted(CourseId, "b", false);
        service.SetPrerequisiteCompleted(CourseId, "b", true);

        Assert.Equal("b", service.Drop(CourseId, "a").PromotedLearnerId);
        AssertState(service, ["b"], ["c"]);
    }

    [Fact]
    public void SkippedLearnerMayRejoinOnlyAtBackAfterEligibilityRestored()
    {
        var service = FullWithWaitlist();
        service.Enroll(CourseId, "d");
        service.SetPrerequisiteCompleted(CourseId, "b", false);
        service.Drop(CourseId, "a");
        service.SetPrerequisiteCompleted(CourseId, "b", true);

        Assert.Equal(RegistrationStatus.Waitlisted, service.Enroll(CourseId, "b").Status);
        AssertState(service, ["c"], ["d", "b"]);
    }

    [Fact]
    public void PrerequisiteRevocationDoesNotEvictExistingEnrollment()
    {
        var service = FullWithWaitlist();
        service.SetPrerequisiteCompleted(CourseId, "a", false);
        service.SetPrerequisiteCompleted(CourseId, "b", false);

        Assert.Equal(RegistrationStatus.AlreadyEnrolled, service.Enroll(CourseId, "a").Status);
        Assert.Equal(RegistrationStatus.AlreadyWaitlisted, service.Enroll(CourseId, "b").Status);
        AssertState(service, ["a"], ["b", "c"]);
        Assert.Equal("c", service.Drop(CourseId, "a").PromotedLearnerId);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("b")]
    [InlineData("c")]
    public void RepeatedDropDoesNotChangeOtherMembership(string learnerId)
    {
        var service = FullWithWaitlist();
        service.Drop(CourseId, learnerId);
        var before = service.GetSnapshot();

        Assert.Equal(new(RegistrationStatus.NotRegistered), service.Drop(CourseId, learnerId));
        AssertState(service, before.EnrolledLearnerIds, before.WaitlistedLearnerIds);
    }

    [Fact]
    public void WaitlistCancellationDoesNotReleaseSeatOrPromoteAnyone()
    {
        var service = FullWithWaitlist();

        Assert.Equal(new(RegistrationStatus.WaitlistCancelled), service.Drop(CourseId, "b"));
        AssertState(service, ["a"], ["c"]);
        service.Enroll(CourseId, "b");
        AssertState(service, ["a"], ["c", "b"]);
    }

    [Fact]
    public void DroppingUnregisteredLearnerDoesNotChangeState()
    {
        var service = FullWithWaitlist();

        Assert.Equal(new(RegistrationStatus.NotRegistered), service.Drop(CourseId, "d"));
        AssertState(service, ["a"], ["b", "c"]);
    }

    [Fact]
    public void DropWithoutWaitlistReleasesExactlyOneSeat()
    {
        var service = Create(capacity: 2);
        service.Enroll(CourseId, "a");
        service.Enroll(CourseId, "b");

        Assert.Equal(new(RegistrationStatus.Dropped), service.Drop(CourseId, "a"));
        AssertState(service, ["b"], []);
    }

    [Fact]
    public void ZeroCapacityNeverEnrollsLearners()
    {
        var service = Create(capacity: 0);

        Assert.Equal(RegistrationStatus.Waitlisted, service.Enroll(CourseId, "a").Status);
        Assert.Equal(RegistrationStatus.WaitlistCancelled, service.Drop(CourseId, "a").Status);
        AssertState(service, [], []);
    }

    [Theory]
    [InlineData(null, "a", RegistrationStatus.InvalidCourseId)]
    [InlineData("", "a", RegistrationStatus.InvalidCourseId)]
    [InlineData("  ", "a", RegistrationStatus.InvalidCourseId)]
    [InlineData("other", "a", RegistrationStatus.CourseNotFound)]
    [InlineData("cs101", "a", RegistrationStatus.CourseNotFound)]
    [InlineData(CourseId, null, RegistrationStatus.InvalidLearnerId)]
    [InlineData(CourseId, "", RegistrationStatus.InvalidLearnerId)]
    [InlineData(CourseId, "\t", RegistrationStatus.InvalidLearnerId)]
    [InlineData(CourseId, "unknown", RegistrationStatus.LearnerNotFound)]
    [InlineData(CourseId, "A", RegistrationStatus.LearnerNotFound)]
    public void InvalidRequestsReturnSpecificResultAndDoNotMutateState(
        string? courseId, string? learnerId, RegistrationStatus expected)
    {
        var service = FullWithWaitlist();

        Assert.Equal(new(expected), service.Enroll(courseId, learnerId));
        Assert.Equal(new(expected), service.Drop(courseId, learnerId));
        Assert.Equal(new(expected), service.SetPrerequisiteCompleted(courseId, learnerId, false));
        AssertState(service, ["a"], ["b", "c"]);
        Assert.Equal("b", service.Drop(CourseId, "a").PromotedLearnerId);
    }

    [Fact]
    public void InitiallyIneligibleLearnerCanEnrollAfterPrerequisiteCompletion()
    {
        var service = Create();
        service.SetPrerequisiteCompleted(CourseId, "ineligible", true);

        Assert.Equal(RegistrationStatus.Enrolled, service.Enroll(CourseId, "ineligible").Status);
    }

    [Fact]
    public void SnapshotCannotMutateServiceAndDoesNotChangeAfterOperations()
    {
        var service = FullWithWaitlist();
        var before = service.GetSnapshot();

        Assert.Throws<NotSupportedException>(() => ((IList<string>)before.EnrolledLearnerIds).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<string>)before.WaitlistedLearnerIds).Clear());
        service.Drop(CourseId, "a");
        Assert.Equal(["a"], before.EnrolledLearnerIds);
        Assert.Equal(["b", "c"], before.WaitlistedLearnerIds);
        AssertState(service, ["b"], ["c"]);
    }

    [Fact]
    public void ConstructorCopiesLearnerInput()
    {
        var learners = new List<Learner> { new("a", true) };
        var service = new RegistrationService(CourseId, 1, learners);
        learners[0] = new("a", false);
        learners.Clear();

        Assert.Equal(RegistrationStatus.Enrolled, service.Enroll(CourseId, "a").Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorRejectsMissingCourseId(string? courseId) =>
        Assert.ThrowsAny<ArgumentException>(() => new RegistrationService(courseId!, 1, []));

    [Fact]
    public void ConstructorRejectsNegativeCapacity() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new RegistrationService(CourseId, -1, []));

    [Fact]
    public void ConstructorRejectsMissingLearners() =>
        Assert.Throws<ArgumentNullException>(() => new RegistrationService(CourseId, 1, null!));

    [Fact]
    public void ConstructorRejectsNullLearner() =>
        Assert.Throws<ArgumentNullException>(() => new RegistrationService(CourseId, 1, [null!]));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorRejectsMissingLearnerId(string? learnerId) =>
        Assert.ThrowsAny<ArgumentException>(() =>
            new RegistrationService(CourseId, 1, [new(learnerId!, true)]));

    [Fact]
    public void ConstructorRejectsDuplicateLearnerIds() =>
        Assert.Throws<ArgumentException>(() =>
            new RegistrationService(CourseId, 1, [new("a", true), new("a", false)]));

    private static RegistrationService Create(int capacity = 1) => new(CourseId, capacity,
    [
        new("a", true), new("b", true), new("c", true), new("d", true), new("ineligible", false)
    ]);

    private static RegistrationService FullWithWaitlist()
    {
        var service = Create();
        service.Enroll(CourseId, "a");
        service.Enroll(CourseId, "b");
        service.Enroll(CourseId, "c");
        return service;
    }

    private static void AssertState(RegistrationService service,
        IEnumerable<string> enrolled, IEnumerable<string> waitlisted)
    {
        var state = service.GetSnapshot();
        Assert.Equal(CourseId, state.CourseId);
        Assert.Equal(enrolled, state.EnrolledLearnerIds);
        Assert.Equal(waitlisted, state.WaitlistedLearnerIds);
        Assert.InRange(state.EnrolledLearnerIds.Count, 0, state.Capacity);
        Assert.Equal(state.EnrolledLearnerIds.Count, state.EnrolledLearnerIds.Distinct().Count());
        Assert.Equal(state.WaitlistedLearnerIds.Count, state.WaitlistedLearnerIds.Distinct().Count());
        Assert.Empty(state.EnrolledLearnerIds.Intersect(state.WaitlistedLearnerIds));
    }
}
