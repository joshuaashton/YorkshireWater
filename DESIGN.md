# Design note

## Architecture

The API has three thin layers, and each has one reason to change:

- **Endpoints** handle HTTP only: binding, validation, and mapping outcomes to status codes.
- **Service** (`NotificationPreferencesService`) owns the business rules: the customer must exist,
  only a real change is stored, and the notifier is called exactly once per change.
- **Infrastructure** provides the in-memory repository and a logging stand-in for the notification
  system. Both sit behind interfaces and are wired up through the built-in DI container.

I chose **minimal APIs** over controllers because the API has two endpoints on one route, so a
controller would add ceremony without adding structure. A route group keeps the mapping in one
place, and typed results (`Results<Ok<T>, ProblemHttpResult>`) make the possible responses explicit.

`NotificationPreferences` is a record, so value equality detects whether anything changed. The
update returns a closed result type (`Updated`, `CustomerNotFound`, `NotificationFailed`) instead of
throwing, so the endpoint has to handle every case.

## Input validation

The request uses `bool?` with `[Required]` and .NET 10's `AddValidation()`, so a missing property
returns a 400 that names it instead of defaulting to `false`. The route has no `:guid` constraint,
so a malformed id is a 400 rather than a 404. The exception handler turns unreadable bodies into
problem details instead of the framework's default empty 400.

## Test approach

I built the API test-first, one small step per commit. Unit tests cover the repository and the
service, using Moq to check interactions such as "notified exactly once". Integration tests boot
the real app with `WebApplicationFactory` and test each acceptance criterion over HTTP; only the
notifier is mocked. Each test gets a fresh app so in-memory state can't leak between tests.

## Failure policy

The service saves first and notifies second. If notification fails, including a timeout, the
save is kept and the client gets a 503. Only a cancelled request propagates. The downside is
that the downstream system can fall out of date, and a retry with the same body won't notify
again because nothing has changed.

## Known limitations

- No authentication or authorisation (out of scope).
- In-memory store: state is lost on restart, and the last write wins.
- The OpenAPI document only lists 200 responses.

## What I would improve

- Split `Domain`, `Services` and `Infrastructure` into separate projects instead of folders in the
  API project, so project references enforce the dependency direction.
- Send notifications through a transactional outbox or a queue with retries, so storage and
  notification can't diverge.
- Use a real database with an update that's conditional on the row still existing, plus ETag and
  `If-Match` for concurrency control.
- Add `ProducesProblem` metadata, health checks and structured telemetry.
