Analyzers
==========

[![Nuget](https://img.shields.io/nuget/v/ILNumerics.Community.Analyzers?style=flat-square&logo=nuget&color=blue)](https://www.nuget.org/packages/ILNumerics.Community.Analyzers)

A set of Roslyn analyzers and code fixes to enforce ILNumerics (http://ilnumerics.net/) function & memory rules (see https://ilnumerics.net/FunctionRules.html).

ILNumerics function rules ensure that APIs using ILNumerics arrays clearly separate inputs, outputs, and locals, and that signatures communicate this intent explicitly. Inputs are passed as `InArray<T>`, outputs as `OutArray<T>`, and return values as `RetArray<T>`. Regular `Array<T>` is reserved for local (or class level) variables. This avoids accidental reuse of result buffers, prevents hidden allocations, and makes data flow and lifetime of ILNumerics arrays obvious and analyzable.

## Getting started

Install the NuGet package `ILNumerics.Community.Analyzers` into any project that should be analyzed:

```shell
dotnet add package ILNumerics.Community.Analyzers
```

- The package contains both analyzers and code fixes.
- Visual Studio and other supported IDEs will run them during typing (live analysis) and on build.

## Rules
- **ILN0001 — Don't use `var` for (local) ILNumerics arrays**

  Implicit typing is forbidden for ILNumerics arrays (`Array<T>`, `InArray<T>`, `OutArray<T>`, `RetArray<T>`, `Logical`, `Cell`, etc.). Use explicit types so readers (and the analyzer) can see whether a variable is an input, output, return buffer, or local `Array<T>`. This prevents subtle bugs where a `RetArray<T>` is reused in unexpected ways.

- **ILN0002 — Only `In/Out/Ret` in signatures (not in bodies/fields/props)**

  `InArray<T>`, `OutArray<T>`, and `RetArray<T>` are meant purely as API "flavors" for parameters and return types. They must not appear as locals, fields, or general properties. Exceptions are very constrained: get‑only `Ret*` properties, set‑only or init‑only `In*` properties are allowed as configuration‑style surfaces, but all other uses in the object model or locals are flagged.

- **ILN0003 — Function signatures should use ILNumerics flavors (in/out parameter and returns)**

  Public/internal methods that expose ILNumerics arrays should not use plain `Array<T>` in signatures. Parameters should be `InArray<T>`/`OutArray<T>` and return values should be `RetArray<T>`. This documents the function contract (who owns the buffer, who may modify it) and enables the runtime and analyzers to enforce correct reuse and memory behavior.

- **ILN0004 — Avoid C# `out`/`ref` with ILNumerics arrays (use `OutArray<T>`)**
  
  ILNumerics discourages C# `ref`/`out` parameters for its array types. Instead of `ref Array<T>` or `out Array<T>`, APIs should use `OutArray<T>` (or the appropriate flavor) and pass arrays in the ILNumerics way. This keeps ownership and reuse semantics consistent and avoids clashes between C# reference semantics and ILNumerics' buffer management model.

- **ILN0005 — ILNumerics fields should use `localMember<T>()` and safe assignment**

  Instance fields of ILNumerics `Array<T>` types are treated as local members: they should be initialized via `localMember<T>()`. `ILN0005` flags non‑static `Array<T>` fields that either have no initializer or use something other than `localMember<T>()`. Valid writes to fields must use either `_A.a = ...` or `_A.Assign(...)`, which keeps the field buffer stable following expected lifetime conventions.

- **ILN0006 — Use `GetValue()`/`SetValue()` for scalar element access**

  Scalar reads from ILNumerics `Array<T>` should use `GetValue(...)` rather than indexer access combined with an explicit cast (for example: `(T) A[i, j]`). `ILN0006` reports cases where an element access is cast to the array's element type, and the code fix rewrites it to `A.GetValue(i, j)` to make scalar access explicit and consistent.

### Troubleshooting

No squiggles but lightbulb available: enable live analysis in the IDE (VS: Tools → Options → Text Editor → C# → Advanced → Enable .NET analyzers, Full solution analysis) and ensure `.editorconfig` does not suppress diagnostics.

### Contributing

Contributions, bug reports and feature requests are welcome. Please open an issue or a pull request on the GitHub repository.

### License

ILNumerics.Community.Analyzers is licensed under the terms of the MIT license (<http://opensource.org/licenses/MIT>, see LICENSE.txt).
