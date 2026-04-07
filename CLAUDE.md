# CLAUDE.md — excel-common

Shared .NET library for Taglo Excel add-ins

## Repo purpose

`Taglo.Excel.Common` — shared ExcelDNA infrastructure extracted from Formula Boss. Provides Win32 interop for window management, cell/window positioning, file-based logging, and GitHub release update checking. Published as a NuGet package to GitHub Packages for consumption by Formula Boss, Lambda Boss, and future Taglo Excel add-ins.

## Tech stack

- C# 10 / .NET 6 (`net6.0-windows`)
- ExcelDna.Integration (compile-time reference)
- xUnit for testing

## Build & test

- `dotnet build taglo-excel-common.sln` — build solution
- `dotnet test taglo-excel-common.sln` — run tests

## Conventions

- `/code-review <pr>` — PR code review
- Specs: `specs/`, Plans: `plans/`
- Default branch: `main`
- **Never use compound Bash commands** (no `&&`, `;`, or `|` chaining). Use separate Bash tool calls instead — independent calls can run in parallel. Compound commands trigger extra permission prompts.
- **Never prefix Bash commands with `cd`**. The working directory is already the project root. All commands (`gh`, `git`, `dotnet`, etc.) work without `cd`.
