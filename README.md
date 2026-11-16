# Course Registration Assessment

A focused C# service for one course offering: enrollment, dropping, prerequisite checks, and FIFO waitlist promotion. It uses an in-memory store and atomic operations. A console demonstration and xUnit tests exercise the behavior.

## Stack and prerequisites

- .NET 10 SDK (the project targets `net10.0`; `global.json` accepts .NET 10 feature-band updates).
- C#, xUnit, and the .NET test runner. The service and demo have no third-party runtime dependencies.
- Internet access to NuGet for the initial test dependency restore.

## Setup, build, and test

From the repository root:

```sh
dotnet restore CourseRegistration.sln
dotnet build CourseRegistration.sln --configuration Release --no-restore
dotnet test CourseRegistration.sln --configuration Release --no-build
dotnet run --project src/CourseRegistration.Demo/CourseRegistration.Demo.csproj --configuration Release --no-build
```

The demo runs a fixed scenario and exits; it does not start a server. To check formatting:

```sh
dotnet format CourseRegistration.sln --verify-no-changes --no-restore
```

## Example operations

```csharp
using CourseRegistration;

var service = new RegistrationService("CS101", 1,
[
    new Learner("alice", true),
    new Learner("bob", true),
    new Learner("carol", true)
]);

service.Enroll("CS101", "alice"); // Enrolled
service.Enroll("CS101", "bob");   // Waitlisted
service.Enroll("CS101", "carol"); // Waitlisted
service.Enroll("CS101", "bob");   // AlreadyWaitlisted; position unchanged
var dropped = service.Drop("CS101", "alice"); // Dropped; PromotedLearnerId == "bob"
var state = service.GetSnapshot(); // enrolled: bob; waitlist: carol
```

The supplied demo additionally revokes Bob's prerequisite before Alice drops. Its actual final output is:

```text
Drop alice: Dropped; promoted carol
Retry alice's drop: NotRegistered
Enrolled: [carol]
Waitlist: []
```

## Policies and limitations

- Only known learners with a completed prerequisite may enter enrollment or the waitlist. IDs are exact, case-sensitive strings; blank and unknown IDs produce distinct outcomes.
- Promotion rechecks prerequisites, removes ineligible entries at the front, and promotes the first eligible learner. Removed learners must complete the prerequisite and enroll again to rejoin at the back.
- A waitlisted drop cancels that entry without releasing a seat. Duplicate enrollment requests and repeated drops leave existing state intact.
- Prerequisite changes do not evict existing enrollments. An already waitlisted learner retains position until cancellation or promotion processing.
- Capacity is fixed and nonnegative; zero capacity only permits waitlisting. One shared service instance must own the offering's state.
- State is lost on restart. Synchronization covers threads sharing that instance, with no support for multiple processes or durable transactions. FIFO uses serialized arrival at the lock; simultaneous callers have no guaranteed ordering.
- No frontend, HTTP API, authentication, database, catalog, or deployment infrastructure is included. Test execution is not a proof of every possible thread interleaving.

## Structure and assessment evidence

| Location | Responsibility |
| --- | --- |
| `src/CourseRegistration` | State, operations, typed results, read-only snapshots |
| `src/CourseRegistration.Demo` | Runnable example |
| `tests/CourseRegistration.Tests` | Business behavior, invalid requests, and concurrency tests |
| [Design](docs/DESIGN.md) | Contracts, invariants, policies, and tradeoffs |
| [Implementation plan](docs/IMPLEMENTATION_PLAN.md) | Approach recorded before implementation |
| [AI usage record](docs/AI_USAGE.md) | Original prompt, contributions, decisions, review, and evidence |
| [Verification](docs/VERIFICATION.md) | Executed checks and acceptance criteria mapping |

Verified on 2026-11-16 with SDK 10.0.203: Release build succeeded with zero warnings/errors; all 46 tests passed with zero failures/skips; the demo and formatting check succeeded. See the verification record for exact commands.
