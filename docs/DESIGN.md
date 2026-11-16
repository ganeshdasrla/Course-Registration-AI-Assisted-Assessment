# Design

## Boundary and responsibilities

One `RegistrationService` owns one offering and its trusted learner roster. The service is an in-process library; the console project supplies an executable example. Tests call the public contract directly. There is no starter implementation or rubric in the supplied workspace, so this uses the PDF's in-memory fallback and the attached prompt's C#/.NET/xUnit conventions.

The service owns validation and registration transitions. `Learner` represents constructor input, `RegistrationResult` describes expected outcomes, and `CourseSnapshot` exposes detached read-only views. Separate storage/repository interfaces or dependency injection would add no useful substitution for this scope.

## Data

- Offering: immutable nonblank course ID and nonnegative fixed capacity.
- Roster: a copied dictionary from unique learner ID to boolean prerequisite-completed status. Immutable learner records prevent external flag mutation; changes go through the service.
- Enrollment: a hash set of learner IDs, which also supplies the occupied-seat count. No separate seat counter can drift.
- Waitlist: a linked list of learner IDs in FIFO order. Only enrollment appends; cancellation removes one entry; promotion consumes the head.
- Snapshot: course ID, capacity, ordinally sorted enrolled IDs, and FIFO waitlisted IDs, wrapped in read-only copies. It cannot mutate the service and does not change after return.

IDs use ordinal, case-sensitive equality. No trimming or normalization occurs. Blank input is rejected, and any nonblank ID must exactly match the constructor's roster/offering.

## Public operations

| Operation | Contract |
| --- | --- |
| `new RegistrationService(courseId, capacity, learners)` | Copies the roster; throws argument exceptions for missing course/roster/learner/learner ID, negative capacity, or duplicate learner IDs. Empty roster and zero capacity are valid. |
| `Enroll(courseId, learnerId)` | Returns `Enrolled`, `Waitlisted`, `AlreadyEnrolled`, `AlreadyWaitlisted`, or `PrerequisiteNotMet`. |
| `Drop(courseId, learnerId)` | Returns `Dropped`, with optional `PromotedLearnerId`; `WaitlistCancelled`; or `NotRegistered`. |
| `SetPrerequisiteCompleted(courseId, learnerId, completed)` | Updates the trusted flag and returns `PrerequisiteUpdated`, even when the value is unchanged. Does not immediately promote or evict anyone. |
| `GetSnapshot()` | Reads this instance's single offering; no course selector is needed. |

All three mutating operations validate in this order: missing/blank course (`InvalidCourseId`), different course (`CourseNotFound`), missing/blank learner (`InvalidLearnerId`), unknown learner (`LearnerNotFound`). Expected request failures return typed results rather than throwing. `PromotedLearnerId` is null except when an enrolled drop actually promotes someone.

## Enrollment

Under the lock, validate IDs, then detect existing enrollment or waitlisting. Retries preserve membership and queue position even after a prerequisite change. For a new request, reject an incomplete prerequisite. If occupied seats are below capacity, insert the learner into enrollment; otherwise append the learner to the waitlist. Duplicate checks precede every insertion.

## Dropping and promotion

Under the same lock, validate IDs. If an enrollment exists, remove it once, then:

1. Read and remove the waitlist head.
2. Recheck its current prerequisite flag.
3. If ineligible, discard it and continue from step 1.
4. Otherwise add it to enrollment, return its ID, and stop.
5. If the queue empties, return no promotion and leave the seat available.

Only one seat was released, so at most one eligible learner is promoted. Later queue entries retain their order. Ineligible entries behind the promoted learner remain queued until their turn is processed. A new enrollment cannot take the released seat between drop and promotion.

If the learner was waitlisted, remove that entry and return `WaitlistCancelled`; no seat is released and no promotion runs. A known learner with no membership returns `NotRegistered`. Repeated drops cannot affect another learner. Rejoining after cancellation or removal goes to the back if the course remains full.

## Eligibility and invariants

The prerequisite rule is the supplied boolean flag. Ineligible learners cannot newly enroll, join the queue, or be promoted. Revoking a prerequisite does not retroactively remove an enrolled learner; enrollment-time eligibility and continued membership are distinct. Waitlisted learners are rechecked on promotion and removed when ineligible. Restoring eligibility before their turn preserves their original position.

Every public state transition preserves:

- `0 <= enrolled count <= capacity`.
- Each learner has at most one enrollment or one queue entry, never both.
- Enrolled and queued IDs always belong to the roster.
- Promotion consumes the earliest currently eligible queue entry.
- Invalid requests and duplicates do not change valid state.

## Concurrency and persistence

One private monitor protects validation, eligibility updates, every state transition, promotion, and snapshots. A request takes effect within that critical section. Callers must share the same service instance for this offering. FIFO is the order in which successful waitlisting operations acquire the lock, rather than wall-clock call start or task scheduling order.

Enrollment lookup is O(1) on average; waitlist duplicate detection/cancellation is O(n). Removing the head is O(1); promotion scanning is O(k) for k consumed entries. Snapshots copy collections and sort enrolled IDs, taking O(e log e + w). This avoids redundant queue indexes while keeping the bounded implementation small.

All data is in memory. There are no cross-process guarantees, durable recovery, or exception rollback for resource exhaustion. Fixed roster and capacity simplify the service boundary. Larger queues could warrant an indexed linked list; durable or distributed use would require a separate persistence/transaction design. These are follow-up considerations outside the assessment.

## Verification

Focused xUnit tests cover all requested behavior, read-only/copy isolation, validation, eligibility restoration, and concurrent enrollment/drop operations. [Verification](VERIFICATION.md) records execution and maps tests to each PDF acceptance criterion. [AI usage](AI_USAGE.md) distinguishes AI contribution, agent review, automated verification, and the absence of independent human review.
