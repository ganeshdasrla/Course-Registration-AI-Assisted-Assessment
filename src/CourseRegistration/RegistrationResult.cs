namespace CourseRegistration;

public enum RegistrationStatus
{
    Enrolled,
    Waitlisted,
    AlreadyEnrolled,
    AlreadyWaitlisted,
    PrerequisiteNotMet,
    Dropped,
    WaitlistCancelled,
    NotRegistered,
    PrerequisiteUpdated,
    InvalidCourseId,
    CourseNotFound,
    InvalidLearnerId,
    LearnerNotFound
}

/// <summary>Expected business outcomes; promotion is reported only for a successful enrolled drop.</summary>
public sealed record RegistrationResult(
    RegistrationStatus Status,
    string? PromotedLearnerId = null);
