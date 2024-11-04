namespace JsonToSql.CLI.Domain.Fields;

internal struct FieldRelationDefinition
{
    public string Name { get; set; }
    public bool IsOneToOne { get; set; }
    public bool IsOneToMany { get; set; }
    public string? OneSide { get; set; }
    public bool IsManyToMany { get; set; }
}