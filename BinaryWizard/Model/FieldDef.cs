namespace BinaryWizard.Model;

public record FieldDef {
    public string Name { get; set; }

    public FieldDef(string name) {
        Name = name;
    }
}