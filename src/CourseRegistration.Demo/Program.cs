using CourseRegistration;

const string courseId = "CS101";
var service = new RegistrationService(courseId, capacity: 1,
[
    new Learner("alice", true),
    new Learner("bob", true),
    new Learner("carol", true),
    new Learner("dave", false)
]);

Print("Enroll alice", service.Enroll(courseId, "alice"));
Print("Enroll bob", service.Enroll(courseId, "bob"));
Print("Enroll carol", service.Enroll(courseId, "carol"));
Print("Retry bob", service.Enroll(courseId, "bob"));
Print("Enroll dave", service.Enroll(courseId, "dave"));
Print("Revoke bob's prerequisite", service.SetPrerequisiteCompleted(courseId, "bob", false));
Print("Drop alice", service.Drop(courseId, "alice"));
Print("Retry alice's drop", service.Drop(courseId, "alice"));

var snapshot = service.GetSnapshot();
Console.WriteLine($"Enrolled: [{string.Join(", ", snapshot.EnrolledLearnerIds)}]");
Console.WriteLine($"Waitlist: [{string.Join(", ", snapshot.WaitlistedLearnerIds)}]");

static void Print(string operation, RegistrationResult result) =>
    Console.WriteLine($"{operation}: {result.Status}" +
        (result.PromotedLearnerId is { } promoted ? $"; promoted {promoted}" : ""));
