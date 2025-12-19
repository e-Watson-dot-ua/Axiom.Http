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

## Building the NuGet package (how-to)

This project is configured to produce a NuGet package. Below are a few ways to build the package and verify the output.

Prerequisites
- .NET SDK installed (recommended: .NET 8). Verify with:

```powershell
dotnet --version
```

Option A — Build (packages generated on build)
- The project sets `GeneratePackageOnBuild` so a `dotnet build -c Release` will produce a `.nupkg` automatically. From the project folder:

```powershell
cd C:\dev\Axiom.Http\src\Axiom.Http
dotnet build -c Release
```

- By default this repository sets the package output path to `C:\dev\Axiom.Http\deploy` (see the `PackageOutputPath` property in `Axiom.Http.csproj`). After the build the package will appear in that folder, for example:

```powershell
Get-ChildItem 'C:\dev\Axiom.Http\deploy' -Filter 'Axiom.Http*.nupkg' | Select-Object Name,Length,LastWriteTime
```

Option B — Pack explicitly
- You can also call `dotnet pack` explicitly (this will ignore `GeneratePackageOnBuild` and produce a package based on pack-time options):

```powershell
dotnet pack "C:\dev\Axiom.Http\src\Axiom.Http\Axiom.Http.csproj" -c Release -o "C:\dev\Axiom.Http\deploy"
```

Verify package contents
- A `.nupkg` file is just a ZIP archive. To inspect its contents (README, nuspec, lib assemblies) you can copy to `.zip` and expand it:

```powershell
Copy-Item 'C:\dev\Axiom.Http\deploy\Axiom.Http.1.0.0.nupkg' 'C:\temp\Axiom.Http.nupkg.zip' -Force
Expand-Archive -Path 'C:\temp\Axiom.Http.nupkg.zip' -DestinationPath 'C:\temp\axpkg'
Get-ChildItem 'C:\temp\axpkg' -Recurse | Select-Object FullName,Length
```

Notes
- The package is already configured to include the repository root `README.md` (packed into the package root as `README.md`) via the project file. If you want a different README or to change what is included, update `Axiom.Http.csproj` and the `None Include` entry for the README.
- To change where the package is written, modify the `PackageOutputPath` property in `Axiom.Http.csproj` (or pass `-o` to `dotnet pack`).
- For CI: prefer `dotnet pack` (or `dotnet build` with `GeneratePackageOnBuild`) in a Release configuration and push the produced `.nupkg` to your feed.

## License

Check the repository root for the project's license file; use the same license when contributing.

---

(Generated on 2025-12-19)
