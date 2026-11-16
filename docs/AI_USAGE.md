# AI-assisted engineering record

Date: 2026-11-16 (Asia/Calcutta).

## Entry 1: Original implementation prompt (verbatim)

The user supplied this prompt as an attachment. Its text is preserved below, including its original encoding artifacts. It is task reference material; the user's direct request to publish to GitHub supersedes its final instruction to leave changes uncommitted locally.

````text
You are a Senior Software Engineer completing an AI-Assisted Engineering Assessment.

Implement the requirements from the attached "Course-Registration-AI-Assisted-Assessment.pdf" in the current repository.

**Goal:** Deliver a clean, production-quality, but minimal implementation of course enrollment, dropping, and FIFO waitlist promotion for a single course offering.

Follow all acceptance criteria, scope boundaries, and deliverables specified in the assessment document.

## 1. Understand the existing project

Before writing code:

- Inspect the repository structure, README, source code, dependencies, and existing tests.
- Identify the existing programming language, framework, architecture, data model, persistence, and testing conventions.
- Reuse existing interfaces and patterns wherever possible.
- Do not introduce unnecessary dependencies or infrastructure.
- If there is no starter implementation, create a minimal C#/.NET service with an in-memory store and focused automated tests.
- Do not create a frontend or a full registration platform.

Briefly document the implementation approach before making changes.

## 2. Functional requirements

Implement the following for one course offering.

### Enrollment

- Allow a learner to enroll when capacity is available and the learner meets the prerequisite.
- Prevent duplicate enrollment.
- When the course is full, add an eligible learner to the waitlist.
- Prevent duplicate waitlist entries.
- An already enrolled or waitlisted learner must not receive duplicate records.
- Return clear results for successful enrollment, waitlisting, duplicate requests, and prerequisite failures.

### Dropping

- Allow an enrolled learner to drop the course.
- Release exactly one occupied seat.
- Automatically promote the earliest eligible learner from the waitlist.
- Preserve FIFO order.
- Handle repeated drops and invalid requests without changing valid state.
- Handle waitlisted learners' drop requests according to a clearly documented policy.

### Prerequisites

Implement a simple prerequisite eligibility rule using the existing project model, if available.

If no rule is provided, assume each learner has a boolean prerequisite-completed status.

For the fallback implementation:

- An ineligible learner cannot enroll or join the waitlist.
- Recheck eligibility before promotion.
- If a waitlisted learner becomes ineligible, skip that learner, remove the entry from the waitlist, and continue searching for the next eligible learner.
- Clearly document this policy and apply it consistently.

Do not build a configurable prerequisite or policy engine.

### Capacity and data integrity

Maintain these invariants:

- Enrolled count must never exceed course capacity.
- A learner can be enrolled only once.
- A learner can appear in the waitlist only once.
- A learner cannot be both enrolled and waitlisted.
- Promotion follows FIFO order among eligible learners.
- Invalid operations must not corrupt state.
- If concurrency is supported, concurrent operations must not cause over-enrollment or duplicate promotion.

Use the starter project's existing transaction or locking mechanism. For an in-memory fallback, implement appropriate synchronization without adding a database.

## 3. Code quality

Follow good software engineering practices:

- Object-oriented design
- SOLID principles where appropriate
- Separation of concerns
- Clear, meaningful naming
- Input validation
- Small, maintainable methods
- Dependency injection only where useful
- No unnecessary abstractions or overengineering

Keep the solution focused on the assessment.

Do not add authentication, payments, a frontend, course catalog, multiple offerings, or deployment infrastructure.

## 4. Automated tests

Use the existing test framework.

If implementing in .NET without an existing framework, use xUnit.

Create focused automated tests covering:

1. Successful enrollment when seats are available.
2. Waitlisting when capacity is full.
3. Duplicate enrollment prevention.
4. Duplicate waitlist prevention.
5. FIFO promotion after a learner drops.
6. Prerequisite failure during enrollment.
7. Ineligible learner handling during promotion.
8. Invalid learner and course requests.
9. Repeated or invalid drops.
10. Capacity and enrollment invariants.
11. Waitlist state after promotion.
12. Concurrent enrollment when practical.

Keep tests independent, readable, and deterministic.

Do not only write tests. Execute them and fix any failures.

## 5. Design specification

Create:

`docs/DESIGN.md`

Document:

- Scope and service boundary
- Architecture and responsibilities
- Data models
- Public operations or API contracts
- Enrollment workflow
- Drop workflow
- FIFO waitlist promotion algorithm
- Capacity and uniqueness invariants
- Prerequisite and ineligible learner policy
- Concurrency and persistence approach
- Assumptions and tradeoffs
- Known limitations and follow-up risks

Keep this document concise and proportional to the assessment.

## 6. AI-assisted engineering record

Create:

`docs/AI_USAGE.md`

This assessment explicitly evaluates responsible AI-assisted development.

Record:

- Exact prompts used for AI assistance
- Relevant AI response excerpts or accurate summaries
- Files and decisions influenced by AI
- Whether each suggestion was accepted, edited, or rejected
- Reasons for significant changes or rejections
- Independent review performed
- Verification evidence, including actual test execution results

Include this original implementation prompt as the first entry.

Record subsequent AI interactions accurately as the work progresses.

Do not invent AI interactions, tests, test results, or review evidence.

## 7. README

Create or update `README.md` with:

- Project overview
- Technology stack
- Prerequisites
- Setup instructions
- Build instructions
- How to run the implementation
- How to execute tests
- Example operations
- Assumptions and limitations
- Links to design and AI usage documentation

Commands must match the actual project.

## 8. Independent verification

After implementation:

1. Review all modified files.
2. Run the build.
3. Run all relevant tests.
4. Inspect the behavior for edge cases.
5. Verify every acceptance criterion against the assessment document.
6. Check for over-enrollment and duplicate records.
7. Confirm FIFO promotion.
8. Check for unnecessary dependencies or out-of-scope features.
9. Record actual commands and results in the documentation.
10. Fix any issues discovered.

If commands cannot run due to environment limitations, clearly identify what could not be verified. Do not claim success without execution evidence.

## 9. Final deliverables

Ensure the repository contains:

- Runnable implementation
- Automated tests
- `docs/DESIGN.md`
- `docs/AI_USAGE.md`
- Updated `README.md`
- Concise limitations or follow-up risks

If the repository contains `Course-Registration-Rubric-Score.md`, review it and align the verification evidence with its relevant scoring criteria. Do not invent missing rubric requirements.

## 10. Final report

After completing the implementation, provide:

1. Summary of implementation.
2. Files created or modified.
3. Design decisions and assumptions.
4. Test execution results.
5. Acceptance criteria checklist with pass/fail/not verified status.
6. Known limitations.
7. Summary of AI-assisted contributions and independent verification.
8. Git status and suggested commit message.

## Important instructions

- Implement the code; do not stop after producing a design.
- Use the existing repository as the source of truth.
- Do not overengineer the solution.
- Prefer correctness and testability over unnecessary features.
- Complete the assessment within the intended 7–8 hour engineering scope.
- Do not create false verification evidence.
- Do not expose credentials or secrets.
- Do not run `git commit` or `git push`.
- Leave all changes locally for my review.

Begin by inspecting the repository, then implement and verify the complete scoped solution.
````

**Response summary:** The coding agent inspected the empty workspace, checked the installed SDK and available Git tools, and extracted the supplied PDF locally with Node.js and zlib after finding no usable Python/PDF CLI. The PDF confirmed the minimal single-offering service/CLI fallback. The agent proposed C#/.NET 10, in-memory state, one synchronization lock, explicit outcomes, FIFO promotion, and xUnit tests. It documented the approach before creating implementation files.

**Disposition and files:** Accepted the scoped fallback in `docs/IMPLEMENTATION_PLAN.md`; implemented the library in `src/CourseRegistration`, demo in `src/CourseRegistration.Demo`, and tests in `tests/CourseRegistration.Tests`. Added solution/build configuration, README, design, this record, and verification evidence. No starter code or rubric was available to reuse.

## Entry 2: Direct user request (verbatim)

```text
AS per the document develope everthing, lets put the code in github.
```

**Response summary:** The agent began implementing the assessment and preparing GitHub publication. It asked for a destination/visibility while continuing development. The workspace had no Git repository or remote and no GitHub CLI.

**Disposition:** Accepted. Publishing is explicitly requested by the user, so the conflicting no-commit/no-push sentence inside the attached prompt does not govern this task. GitHub operations use the existing Git Credential Manager sign-in; no credential values are written to source, documentation, or tool output.

## Entry 3: Publication clarification (verbatim)

```text
yes public in my github
```

**Response summary:** The agent selected public visibility. A credential-manager account listing and GitHub `/user` request confirmed the signed-in account; a read-only repository query confirmed that the workspace-named repository did not yet exist. The agent chose `Course-Registration-AI-Assisted-Assessment` as the repository name and communicated the destination.

**Disposition:** Accepted public publication. GitHub publishing evidence is recorded separately after the operation actually completes.

## Engineering decisions and edits

| AI contribution / generated material | Disposition | Review and reason |
| --- | --- | --- |
| One lock around request validation, enrollment, drops, prerequisite updates, and snapshots | Accepted | Keeps capacity check plus insertion, and removal plus promotion, atomic. Same state is never modified without the lock. |
| Hash-set enrollment plus linked-list queue | Accepted | Unique enrollment, FIFO head removal, and simple cancellation without redundant seat counters or queue indexes. Linear queue membership/cancellation is acceptable for this bounded service. |
| Boolean prerequisite, skip/remove ineligible queued learners at promotion | Accepted | Matches the supplied fallback policy; tests cover revocation, restoration, all-ineligible queues, and rejoining at the back. |
| Typed outcomes and read-only copied snapshots | Accepted | Expected invalid requests avoid exceptions; callers cannot mutate service state through returned collections. |
| SDK-generated xUnit project and dependencies | Edited | Removed the template's unused `coverlet.collector`; retained xUnit, its test adapter, and Microsoft.NET.Test.Sdk. Runtime projects have no third-party dependencies. |
| SDK-generated placeholder class/test/demo | Replaced | Replaced template placeholders with the actual implementation, focused tests, and runnable scenario. |
| Functional and concurrent test implementation | Accepted after execution | Reviewed assertions against requirements rather than implementation internals; queued arrivals use the captured queue order instead of assuming task scheduling order. |
| Public publishing despite attached prompt's local-only sentence | Accepted under direct user instructions | User requested GitHub and explicitly selected public visibility. |

Two initial patch attempts failed because the SDK-generated demo text did not match the expected placeholder/BOM context. The failed patches made no implementation changes. The agent then removed and recreated the generated placeholder. No test run failed; the first executed Release build and test run passed. Python extraction was unavailable, and the first inline Node command failed because PowerShell stripped quotes; a here-string piped to Node successfully decoded the local PDF. These setup corrections are not presented as test failures.

## Review and verification

The same coding agent reviewed the generated source against the PDF and the attached requirements. It traced each public transition, capacity and uniqueness invariants, exact validation outcomes, FIFO order, eligibility changes, input isolation, and read-only snapshots. It also reviewed dependency/scope boundaries and documentation commands. There was no second AI agent, external model prompt, or independent human review. This record does not claim that agent self-review constitutes independent human approval.

Verification relied on actual process execution and assertions against public behavior:

- `dotnet restore CourseRegistration.sln`: exit 0; three projects restored.
- `dotnet build CourseRegistration.sln --configuration Release --no-restore`: exit 0; zero warnings/errors.
- `dotnet test CourseRegistration.sln --configuration Release --no-build --logger "trx;LogFileName=registration-tests.trx" --results-directory .artifacts/TestResults`: exit 0; 46 passed, zero failed/skipped; reported test duration 184 ms.
- `dotnet run --project src/CourseRegistration.Demo/CourseRegistration.Demo.csproj --configuration Release --no-build`: exit 0; Bob was skipped after prerequisite revocation, Carol promoted, repeated Alice drop returned `NotRegistered`, final queue empty.
- `dotnet format CourseRegistration.sln --verify-no-changes --no-restore`: exit 0; no formatting changes needed.

The three concurrency tests exercise 300 enrollment calls for 100 learners, 100 duplicate drops of a single enrollment, and distinct drops racing new enrollment. They check exact outcome counts, disjoint/unique memberships, capacity, remaining FIFO priority, and single promotion. The enrollment test additionally drains the queue in its observed serialized arrival order.

[VERIFICATION.md](VERIFICATION.md) maps all seven PDF acceptance criteria and supplemental requested checks to actual evidence and identifies unverified human/rubric review. The local ignored TRX result contains machine-specific paths; no test artifacts or credentials are intended for publication. Only Windows/.NET 10 execution was performed; no cross-platform or exhaustive scheduling proof is claimed.

## Publication result

Following the user's public-publication instruction, the agent initialized Git, committed the reviewed deliverables, created https://github.com/ganeshdasrla/Course-Registration-AI-Assisted-Assessment as a public repository, and pushed main. GitHub's commit API confirmed the initial implementation SHA be8b99ae0292aee38581f755e0ddc99c9504d2fe matched local HEAD. Git staging whitespace checks passed and generated/artifact paths were excluded. The local source scan found no credential-pattern matches, and the original prompt passed an exact-content check before staging. This entry was written after successful publication and is included in a follow-up documentation commit.
