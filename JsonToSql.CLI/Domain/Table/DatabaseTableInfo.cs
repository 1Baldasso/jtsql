namespace JsonToSql.CLI.Domain.Table;

public struct DatabaseTableInfo
{
    public string Name { get; set; }
    public ICollection<DatabaseFieldInfo> Fields { get; set; }
    public ICollection<DatabaseIndex> Indexes { get; set; }
}