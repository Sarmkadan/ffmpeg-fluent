# Contributing to FFmpegFluent

Thanks for helping out. This guide covers how to set up, build, test, and submit changes.

## Prerequisites

- **.NET SDK 10.0** (the library and test project target `net10.0`; there is no `global.json`).
- **ffmpeg and ffprobe** on your `PATH`, or pointed to with the `FFMPEG_PATH` environment variable.
  On Debian/Ubuntu: `sudo apt install ffmpeg`. The integration tests in `test/` only run when
  `FFMPEG_PATH` and `FFPROBE_PATH` are set, and print `[SKIP]` otherwise.

## Build

```bash
dotnet build ffmpeg-fluent.csproj -c Release
```

The library has no NuGet dependencies besides `Microsoft.Extensions.Logging.Abstractions` and the
SourceLink package. `GenerateDocumentationFile` is on, so every public member needs XML docs or the
build warns.

## Tests

```bash
dotnet test test/FFmpegFluent.Tests.csproj -c Release
```

**Note:** the test project is not wired to a test framework yet. The test classes in `test/` are
static classes with `RunAll()` / `RunTests()` methods that print results to the console. Because
nothing calls them, `dotnet test` currently runs no tests and exits with code 0. To run a suite,
reference the library from a throwaway console project and call the method, for example
`FFmpegLocatorTests.RunAll()`. Moving these to xUnit or MSTest is welcome, but please mention it in
the PR if you start that work.

## Coding conventions

There is no `.editorconfig` and no analyzers, so follow the existing code:

- File-scoped namespaces, 4-space indentation, `#nullable enable`.
- One public type per file in `src/`.
- XML doc comments on all public API.
- Companion-file pattern: sugar goes in `XExtensions.cs`, JSON round-trip in `XJsonExtensions.cs`,
  non-throwing checks in `XValidation.cs`.
- Fluent methods return `this` (or the extended instance).
- Numeric parsing and formatting use `CultureInfo.InvariantCulture`.
- Every public API change updates the matching `docs/<TypeName>.md` and the relevant section of
  `README.md`.

## Branches and pull requests

- Branch from `main`. Use a short prefix that matches the commit type: `feat/`, `fix/`, `docs/`,
  `chore/`, for example `fix/locator-exe-suffix`.
- Keep PRs small and focused: one change or one fix per PR. Split unrelated work into separate PRs.
- Use conventional commit prefixes (`fix:`, `feat:`, `docs:`, `chore:`).
- Add or update tests for every behavior change. A bug fix should include a test that fails without
  the fix.
- Make sure `dotnet build` passes with no new warnings, and that CI is green before you ask for
  review. CI runs on every push and PR to `main` (`.github/workflows/ci.yml`).
- In the PR description, explain what changed and why, and list how you tested it.

## Reporting issues

Open an issue at <https://github.com/Sarmkadan/ffmpeg-fluent/issues> and include:

- The library version and .NET SDK version (`dotnet --info`).
- Your OS and the output of `ffmpeg -version`.
- A minimal code sample that reproduces the problem.
- For failed runs, the `FFmpegException` details: exit code, command line, and stderr.

For security problems, do not open a public issue. Contact the maintainer privately instead.
