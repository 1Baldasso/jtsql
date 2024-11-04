namespace JsonToSql.CLI.Domain;

public struct DatabaseFieldInfo
{
    public string Name { get; set; }
    public string ClrType { get; set; }
    public string DatabaseType { get; set; }
    public bool IsNullable { get; set; }
    public bool IsAutoIncrement { get; set; }
    public bool IsPk { get; set; }
}