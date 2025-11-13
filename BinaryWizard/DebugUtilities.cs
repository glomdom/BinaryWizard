using System.Diagnostics;
using BinaryWizard.Model;

namespace BinaryWizard;

public static class DebugUtilities {
    [Conditional("DEBUG")]
    public static void CreatedFieldDef(FieldDef def) {
        Debug.WriteLine($"Created field definition {def.Name} of {def.TypeModel.Type} ({def.ByteSize} bytes) (dynamic? {def.IsDynamic})");
    }
}