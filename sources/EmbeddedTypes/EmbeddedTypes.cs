using System.ComponentModel;
using Microsoft.CodeAnalysis;

[module: ExportEmbeddedType(typeof(Microsoft.CodeAnalysis.EmbeddedAttribute))]
[module: ExportEmbeddedType(typeof(System.Diagnostics.CodeAnalysis.UnscopedRefAttribute))]
[module: ExportEmbeddedType(typeof(System.Runtime.CompilerServices.ScopedRefAttribute))]

[AttributeUsage(AttributeTargets.Module, AllowMultiple = true)]
internal sealed class ExportEmbeddedTypeAttribute(Type type) : Attribute;

namespace Microsoft.CodeAnalysis
{
    [Embedded]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    internal sealed class EmbeddedAttribute : Attribute;
}

namespace System.Diagnostics.CodeAnalysis
{
    [Embedded]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class UnscopedRefAttribute : Attribute;
}

namespace System.Runtime.CompilerServices
{
    [Embedded]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class ScopedRefAttribute : Attribute;
}
