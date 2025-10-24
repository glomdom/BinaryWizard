using Microsoft.CodeAnalysis;

namespace BinaryWizard;

internal static class Diagnostics {
    internal static readonly DiagnosticDescriptor MissingFileError = new DiagnosticDescriptor(
        id: "BINWZ0001",
        title: "Missing nif.xml",
        messageFormat: "Required configuration file 'nif.xml' was not found in the project",
        category: "BinaryWizard",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}