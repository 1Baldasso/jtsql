namespace JsonToSql.CLI.Domain;

public struct DatabaseIndex
{
    public DatabaseFieldInfo Field { get; set; }
    public bool IsPrimaryKey { get; set; }
    public string? FieldsNames { get; set; }
    public bool IsForeignKey { get; set; }
    public string? ReferencesTable { get; set; }
    public string? ReferencesField { get; set; }
    public string Name { get; set; }
    public bool IsUnique { get; set; }
}