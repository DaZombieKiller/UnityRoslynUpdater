# Unity Roslyn Updater
A tool to update the Roslyn compiler and C# language versions for a Unity installation.

# Usage
**NOTE**: This will modify your Unity installation folder, administrative privileges are required!

`UnityRoslynUpdater.exe <path to Unity Editor folder>`

For example: `UnityRoslynUpdater.exe "C:\Program Files\Unity\Hub\Editor\2022.3.8f1\Editor"`

# Language Support
* Working
  * Feature works exactly as expected.
* PolySharp
  * Feature works when using [PolySharp](https://github.com/Sergio0694/PolySharp) and/or manually implementing missing APIs.
* Not Supported
  * Requires runtime features or BCL changes that Unity does not have. Attempting to use the feature may result in compiler errors.
* Crash
  * Requires runtime features that Unity does not have. Attempting to use the feature may compile, but will result in crashes.

## C# 13
Feature | Status
-|-
[params collections](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#params-collections) | Working
[New lock type and semantics](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#new-lock-object) | Not Supported
[New escape sequence - \\e](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#new-escape-sequence) | Working
[Method group natural type improvements](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#method-group-natural-type) | Working
[Implicit indexer access in object initializers](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#implicit-index-access) | Working
[Enable ref locals and unsafe contexts in iterators and async methods](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-and-unsafe-in-iterators-and-async-methods) | Working
[Enable ref struct types to implement interfaces](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-struct-interfaces) | Not Supported
[Allow ref struct types as arguments for type parameters in generics](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#allows-ref-struct) | Not Supported
[Partial properties and indexers](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#more-partial-members) | Working
[Overload resolution priority](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#overload-resolution-priority) | PolySharp
[The `field` keyword](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#the-field-keyword) | Working

## C# 12
Feature | Status
-|-
[Primary constructors](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#primary-constructors) | Working
[Optional parameters in lambda expressions](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#default-lambda-parameters) | Working
[Alias any type](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#alias-any-type) | Working
[Inline arrays](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#inline-arrays) | Not Supported
[Collection expressions](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#collection-expressions) | Working
[Interceptors](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#interceptors) | Not Supported

## C# 11
Feature | Status
-|-
[Raw string literals](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#raw-string-literals) | Working
[`static abstract`/`static virtual` members in interfaces](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support) | Not Supported
[Checked user defined operators](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support) | Working
[Relaxed shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support) | Working
[Unsigned right-shift operator](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-math-support) | Working
[Generic attributes](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#generic-attributes) | Crash
[UTF-8 string literals](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#utf-8-string-literals) | Working
[Newlines in string interpolations](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#newlines-in-string-interpolations) | Working
[List patterns](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#list-patterns) | Working
[File-local types](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#file-local-types) | Working
[Required members](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#required-members) | PolySharp
[Auto-default structs](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#auto-default-struct) | Working
[Pattern match `Span<char>` or `ReadOnlySpan<char>` on a constant `string`](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#pattern-match-spanchar-or-readonlyspanchar-on-a-constant-string) | Working
[Extended nameof scope](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#extended-nameof-scope) | Working
[Numeric `IntPtr` and `UIntPtr`](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#numeric-intptr-and-uintptr) | Working
[`ref` fields](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#ref-fields-and-ref-scoped-variables) | Not Supported
[`ref scoped` variables](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#ref-fields-and-ref-scoped-variables) | PolySharp
[Improved method group conversion to delegate](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#improved-method-group-conversion-to-delegate) | Working

## C# 10
Feature | Status
-|-
[Record structs](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#record-structs) | Working
[Improvements of structure types](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#improvements-of-structure-types) | Working
[Interpolated string handler](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#interpolated-string-handler) | PolySharp
[Global using directives](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#global-using-directives) | Working
[File-scoped namespace declaration](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#file-scoped-namespace-declaration) | Working<sup>1</sup>
[Extended property patterns](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#extended-property-patterns) | Working
[Lambda expression improvements](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#lambda-expression-improvements) | Working
[Constant interpolated strings](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#constant-interpolated-strings) | Working
[Record types can seal ToString](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#record-types-can-seal-tostring) | Working
[Assignment and declaration in same deconstruction](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#assignment-and-declaration-in-same-deconstruction) | Working
[Improved definite assignment](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#improved-definite-assignment) | Working
[Allow AsyncMethodBuilder attribute on methods](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#allow-asyncmethodbuilder-attribute-on-methods) | Not Supported<sup>2</sup>
[CallerArgumentExpression attribute](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#callerargumentexpression-attribute-diagnostics) | PolySharp
[Enhanced #line pragma](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#enhanced-line-pragma) | Working

1. Unity 2021 and earlier require [UnityNamespacePatch](https://github.com/DaZombieKiller/UnityNamespacePatch) to be installed.
2. `AsyncMethodBuilderAttribute` requires changes to its `[AttributeUsage]` attribute for this to work.
