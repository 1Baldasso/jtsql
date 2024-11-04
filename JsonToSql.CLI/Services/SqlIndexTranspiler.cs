using JsonToSql.CLI.Commands.Settings;
using JsonToSql.CLI.Domain;
using JsonToSql.CLI.Domain.Fields;
using JsonToSql.CLI.Providers;
using System.Text.Json;

namespace JsonToSql.CLI.Services;

internal interface ISqlIndexTanspiler
{
    void UpdateIndexes(IDictionary<string, JsonElement> dic,
        FieldsTranslatedToTableDescription root,
        ICollection<FieldsTranslatedToTableDescription> children);
}

internal class SqlIndexTranspiler(ISettingsProvider<CommonSettings> settings) : ISqlIndexTanspiler
{
    private readonly ISettingsProvider<CommonSettings> _settings = settings;

    public void UpdateIndexes(IDictionary<string, JsonElement> dic, FieldsTranslatedToTableDescription root, ICollection<FieldsTranslatedToTableDescription> children)
    {
        var toAdd = new List<FieldsTranslatedToTableDescription>();
        foreach (var c in children)
        {
            var toCheck = c.Fields.Where(x => x.IsOneToOne || x.IsOneToMany || x.IsManyToMany);
            var tablePk = c.Table.Indexes.FirstOrDefault(x => x.IsPrimaryKey).Field;
            var rootPk = root.Table.Indexes.FirstOrDefault(x => x.IsPrimaryKey).Field;
            foreach (var field in toCheck)
            {
                if (field.IsOneToOne)
                {
                    var itemId = new DatabaseFieldInfo
                    {
                        Name = $"{field.Name}_{root.Table.Name}_id",
                        ClrType = tablePk.ClrType,
                        DatabaseType = tablePk.DatabaseType == "serial" ? "integer" : tablePk.DatabaseType,
                        IsAutoIncrement = false,
                        IsNullable = true,
                    };
                    root.Table.Fields.Add(itemId);
                    root.Table.Indexes.Add(new DatabaseIndex
                    {
                        IsForeignKey = true,
                        ReferencesField = tablePk.Name,
                        ReferencesTable = c.Table.Name,
                        Name = $"FK_{c.Table.Name}_{root.Table.Name}_{c.Table.Name}{tablePk.Name}_{field.Name}_id",
                        Field = itemId,
                    });
                    var rootId = new DatabaseFieldInfo
                    {
                        Name = $"{root.Table.Name}_{field.Name}_id",
                        ClrType = rootPk.ClrType,
                        DatabaseType = tablePk.DatabaseType == "serial" ? "integer" : tablePk.DatabaseType,
                        IsAutoIncrement = false,
                        IsNullable = true,
                    };
                    c.Table.Fields.Add(rootId);
                    c.Table.Indexes.Add(new DatabaseIndex
                    {
                        IsForeignKey = true,
                        ReferencesField = rootPk.Name,
                        ReferencesTable = root.Table.Name,
                        Name = $"FK_{root.Table.Name}_{c.Table.Name}_{root.Table.Name}{rootPk.Name}_{field.Name}_id",
                        Field = rootId
                    });
                }
                else if (field.IsOneToMany)
                {
                    if (field.OneSide == _settings.Settings!.RootTableName)
                    {
                        var fieldId = new DatabaseFieldInfo
                        {
                            Name = $"{field.Name}_{root.Table.Name}_id",
                            ClrType = rootPk.ClrType,
                            DatabaseType = rootPk.DatabaseType == "serial" ? "integer" : rootPk.DatabaseType,
                            IsAutoIncrement = false,
                            IsNullable = true,
                        };
                        c.Table.Fields.Add(fieldId);
                        c.Table.Indexes.Add(new DatabaseIndex
                        {
                            IsForeignKey = true,
                            ReferencesField = root.Table.Indexes.FirstOrDefault(x => x.IsPrimaryKey).Field.Name,
                            ReferencesTable = root.Table.Name,
                            Field = fieldId,
                            Name = $"FK_{root.Table.Name}_{c.Table.Name}_{root.Table.Name}{rootPk.Name}_{field.Name}_id",
                        });
                    }
                    else
                    {
                        var table = children.FirstOrDefault(x => x.Fields.Select(x => x.Name).Contains(field.OneSide));
                        var tpk = table.Table.Indexes.FirstOrDefault(x => x.IsPrimaryKey).Field;
                        var fieldId = new DatabaseFieldInfo
                        {
                            Name = $"{field.Name}_{table.Table.Name}_id",
                            ClrType = tpk.ClrType,
                            DatabaseType = tpk.DatabaseType == "serial" ? "integer" : tpk.DatabaseType,
                            IsAutoIncrement = false,
                            IsNullable = true,
                        };
                        root.Table.Fields.Add(fieldId);
                        root.Table.Indexes.Add(new DatabaseIndex
                        {
                            IsForeignKey = true,
                            ReferencesField = table.Table.Indexes.FirstOrDefault(x => x.IsPrimaryKey).Field.Name,
                            ReferencesTable = table.Table.Name,
                            Field = fieldId,
                            Name = $"FK_{table.Table.Name}_{root.Table.Name}_{table.Table.Name}{tpk.Name}_{field.Name}_id",
                        });
                    }
                }
                else if (field.IsManyToMany)
                {
                    var rootId = new DatabaseFieldInfo
                    {
                        Name = rootPk.Name,
                        ClrType = rootPk.ClrType,
                        DatabaseType = tablePk.DatabaseType == "serial" ? "integer" : tablePk.DatabaseType,
                        IsNullable = false
                    };
                    var itemId = new DatabaseFieldInfo
                    {
                        Name = tablePk.Name,
                        ClrType = tablePk.ClrType,
                        DatabaseType = tablePk.DatabaseType == "serial" ? "integer" : tablePk.DatabaseType,
                        IsNullable = false,
                    };
                    var itemToAdd = new FieldsTranslatedToTableDescription
                    {
                        Fields = [
                            new FieldRelationDefinition {
                                IsManyToMany = true,
                                Name = root.Table.Name
                            },
                            new FieldRelationDefinition {
                                IsManyToMany = true,
                                Name = c.Table.Name
                            }
                        ],
                        Table = new Domain.Table.DatabaseTableInfo
                        {
                            Name = c.Table.Name + root.Table.Name,
                            Fields = [
                                rootId,
                                itemId,
                            ],
                            Indexes = [
                                new DatabaseIndex {
                                    IsForeignKey = true,
                                    Field = rootId,
                                    Name = $"FK_{root.Table.Name}_{rootPk.Name}_{rootId.Name}",
                                    ReferencesField = rootPk.Name,
                                    ReferencesTable = root.Table.Name
                                },new DatabaseIndex {
                                    IsForeignKey = true,
                                    Field = itemId,
                                    Name = $"FK_{c.Table.Name}_{tablePk.Name}_{itemId.Name}",
                                    ReferencesField = tablePk.Name,
                                    ReferencesTable = c.Table.Name
                                },
                                new DatabaseIndex
                                {
                                    Field = rootId,
                                    IsPrimaryKey = true,
                                    IsForeignKey = false,
                                    Name = $"PK_{c.Table.Name + root.Table.Name}",
                                    FieldsNames = $"{rootId.Name}, {itemId.Name}"
                                },
                            ]
                        },
                    };
                    if (!toAdd.Any(x => x.Table.Name == itemToAdd.Table.Name))
                        toAdd.Add(itemToAdd);
                }
            }
        }
        foreach (var item in toAdd)
        {
            children.Add(item);
        }
    }
}