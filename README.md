# Axiom.Http

A lightweight HTTP client / response helpers used across the Axiom solution.

This project contains small types and extension methods for working with API responses (success/error), paged data, and helper conversions used by higher-level services.

## Project layout

- `Responses/`
  - `ApiResponse.cs` — core response model used by APIs (success, error, status, payload).
  - `ApiResponseExtensions.cs` — extension helpers for converting and working with `ApiResponse` instances.
  - `PagedResponse.cs` — typed representation for paged lists of results.
- `Axiom.Http.csproj` — project file.
- `Axiom.Http.Tests/` — unit tests covering the response helpers.

## Goals

- Provide a single, consistent response shape for HTTP-facing code in the solution.
- Small, testable helpers for mapping and transforming responses.
- Keep dependencies minimal and focused on .NET SDK types.

## Build

Requires .NET 8 SDK (or the SDK targeted by the solution).

From the repo root (where `Axiom.sln` is located):

```powershell
dotnet build Axiom.sln --configuration Debug
```

Or build only this project:

```powershell
dotnet build Axiom.Http\Axiom.Http.csproj --configuration Debug
```

## Test

Run tests for the HTTP project:

```powershell
dotnet test Axiom.Http.Tests\Axiom.Http.Tests.csproj --configuration Debug
```

## Usage

The `Responses` types are intentionally small and straightforward. Typical usage patterns:

- Create a successful response with a payload:

```csharp
var response = ApiResponse.Success(payload);
```

- Create an error response with message(s):

```csharp
var response = ApiResponse.Failed("Validation failed");
```

- Map or transform payloads using extension helpers in `ApiResponseExtensions`:

```csharp
// Example: convert payload to another type or to a standardized error
var mapped = response.MapPayload(old => newDto);
```

- Work with paged results using `PagedResponse<T>`:

```csharp
var page = new PagedResponse<User>(items, pageIndex: 1, pageSize: 20, totalCount: 200);
```

Refer to unit tests in `Axiom.Http.Tests` for concrete examples of helper usage.

## Extending Responses

When adding new convenience methods or new response types:

- Keep helpers small and focused — prefer single-purpose extension methods.
- Avoid duplicating conversion logic — centralize common behavior in private helpers or internal utility methods.
- Add unit tests for every public helper and for edge cases (null payloads, empty collections, large counts).

## Contributing

- Follow the solution's coding style and global usings.
- Add tests for all new behavior and ensure `dotnet test` is green.
- Keep changes small and well-scoped; prefer a separate PR per logical change.

## Notes / Next Steps

- Consider renaming or reorganizing extension methods if duplication is found across projects (move common utilities to `Axiom.Core` or another shared package).
- Add XML docs to public types to improve discoverability in IDEs.
- If you want, I can also refactor `ApiResponseExtensions` to remove duplication and add tests — let me know and I will do that next.

## License

Check the repository root for the project's license file; use the same license when contributing.

---

(Generated on 2025-12-19)

