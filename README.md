# ILNumerics Roslyn analyzers

A set of Roslyn analyzers and code fixes to enforce ILNumerics function & memory rules.

## Usage

### Consume via NuGet (recommended)
Install the NuGet package `ILNumerics.Community.Analyzers` into any project that should be analyzed:

```shell
dotnet add package ILNumerics.Community.Analyzers
```

- The package contains both analyzers and code fixes.
- Visual Studio and other supported IDEs will run them during typing (live analysis) and on build.
- Ensure IDE live analysis is enabled (VS: Tools → Options → Text Editor → C# → Advanced → Enable .NET analyzers, Full solution analysis).

### Reference directly from source (development/testing)
If you are working in this repository and want analyzers/codefixes active in another project (e.g., `Playground`), reference the analyzer and codefix projects as analyzer references:

```xml
<ItemGroup>
  <ProjectReference Include="..\Analyzers.Analyzers\Analyzers.Analyzers.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
  <ProjectReference Include="..\Analyzers.CodeFixes\Analyzers.CodeFixes.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

Alternatively, reference built analyzer DLLs explicitly:

```xml
<ItemGroup>
  <Analyzer Include="..\Analyzers.Analyzers\bin\$(Configuration)\netstandard2.0\ILNumerics.Community.Analyzers.Analyzers.dll" />
  <Analyzer Include="..\Analyzers.CodeFixes\bin\$(Configuration)\netstandard2.0\ILNumerics.Community.Analyzers.CodeFixes.dll" />
</ItemGroup>
```

## Rules
- ILN0001 — Don't use `var` for (local) ILNumerics arrays
- ILN0002 — Only `In/Out/Ret` in signatures (not in bodies/fields/props)
- ILN0003 — Function signatures should use ILNumerics flavors (in/out parameter and returns)
- ILN0004 — Avoid C# `out/ref` with ILNumerics arrays (use `OutArray<>`)
- ILN0005 — Assign to `OutArray<>` via `.a =` or indexing

## Projects
- **Analyzers.Analyzers** — Analyzers / Diagnostics
- **Analyzers.CodeFixes** — Code Fixes for the analyzers
- **Analyzers** — Packaging project that produces the NuGet with both analyzers and code fixes

## Troubleshooting
- No squiggles but lightbulb available: enable live analysis in the IDE and ensure `.editorconfig` does not suppress diagnostics.

### License

ILNumerics.Community.Analyzers is licensed under the terms of the MIT license (<http://opensource.org/licenses/MIT>, see LICENSE.txt).
