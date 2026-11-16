# Implementation approach

Recorded before implementation on 2026-11-16.

The workspace contains no starter files, Git metadata, existing tests, or rubric. The supplied PDF permits a small service or CLI with in-memory storage. The supplied implementation prompt specifies C#/.NET and xUnit for this fallback.

1. Create a .NET 10 solution with a domain service, console demonstration, and xUnit tests. Use only test dependencies; no web server, frontend, database, or deployment infrastructure.
2. Give one service instance ownership of one offering, known learners, prerequisite flags, enrolled IDs, and an ordered waitlist. Guard all state reads and writes with one lock. Return detached, read-only snapshots.
3. Validate course and learner identifiers before mutations. Return typed outcomes for expected failures. Treat duplicate requests as no-ops. Allow waitlist cancellation without freeing a seat.
4. On an enrolled drop, remove exactly one enrollment and scan the waitlist from the front. Remove ineligible entries and promote at most one eligible learner. Eligibility updates use the same lock.
5. Test all PDF acceptance criteria plus construction validation, snapshot isolation, cancellation/rejoining, all-ineligible queues, and concurrent enrollment/drop behavior.
6. Build, run tests, execute the demo, review the source against the PDF, and record actual evidence in the design, AI usage, verification, and README documents.
7. Prepare Git and publish to the user's GitHub destination when destination and authentication are available. The user's direct publishing request takes precedence over the attached prompt's instruction to leave changes uncommitted locally.

AI generates implementation and test suggestions. Automated execution and an explicit source review verify them; no separate human review or independent reviewer is claimed.
