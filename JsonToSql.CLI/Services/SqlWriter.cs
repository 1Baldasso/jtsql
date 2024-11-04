using JsonToSql.CLI.Commands.Settings;
using JsonToSql.CLI.Domain;
using JsonToSql.CLI.Providers;
using Spectre.Console;
using System.Text;

namespace JsonToSql.CLI.Services;

internal interface ISqlWriter
{
    Task<string> WriteResult(ICollection<FieldsTranslatedToTableDescription> transpiledTables, CancellationToken cancellationToken);
}

internal class SqlWriter : ISqlWriter
{
    private readonly ISettingsProvider<CommonSettings> _settings;

    public SqlWriter(ISettingsProvider<CommonSettings> settings)
    {
        _settings = settings;
    }

    public async Task<string> WriteResult(ICollection<FieldsTranslatedToTableDescription> transpiledTables, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        if (_settings.Settings.Verbose)
            sb.AppendLine();

        if (_settings.Settings?.Describe ?? false)
        {
            //Describe
            sb.AppendLine($"""
            /*
            The json file found in the path {_settings.Settings.Path} was read and the following fields where found to have possible table structures.
            Fields with the same structure area grouped inside square brackets "[]"
                {string.Join("\n    ", transpiledTables.Select(t => $"[{string.Join(',', t.Fields.Select(z => z.Name))}]"))}

            The fields were generated automatically based on Json Parsing strategies, considering author's most common data-types used.
            The configurations taken into consideration while generating data were
                root-name: {_settings.Settings.RootTableName}
                auto-increment enabled: {(_settings.Settings.AutoIncrement ? "yes" : "no")}
                key-type: {_settings.Settings.KeyType}

            The indexes were created taking the following into consideration:
            {string.Join("\n\n", transpiledTables.SelectMany(x => x.Fields).Select(x => $"""
                    Field name: {x.Name}
                        Is One to One: {(x.IsOneToOne ? "yes" : "no")}
                        Is One to Many: {(x.IsOneToMany ? $"yes, with one side being considered {x.OneSide}" : "no")}
                        Is Many to Many: {(x.IsManyToMany ? "yes" : "no")}
                """))}
            */

            """);
        }

        // Create Tables
        var uuidAutoincrement = _settings.Settings.AutoIncrement
            && _settings.Settings.KeyType.Equals("uuid", StringComparison.OrdinalIgnoreCase)
            ? " DEFAULT gen_random_uuid()"
            : string.Empty;

        foreach (var table in transpiledTables)
        {
            sb.AppendLine($"CREATE TABLE IF NOT EXISTS {table.Table.Name} (");
            sb.Append("    ");
            sb.AppendJoin(",\n    ", table.Table.Fields.OrderByDescending(x => x.IsPk).Select(x =>
                $"{x.Name} {(!x.IsPk ? $"{x.DatabaseType}" : $"{x.DatabaseType}{uuidAutoincrement} PRIMARY KEY")}{(!x.IsNullable && !x.IsPk ? " NOT NULL" : "")}"));
            sb.AppendLine("\n);");
            sb.AppendLine();
        }
        // Create Indexes
        foreach (var table in transpiledTables.Where(x => x.Fields.Any(x => x.IsManyToMany) && x.Table.Indexes.Any(x => x.IsPrimaryKey && x.FieldsNames != null)))
        {
            sb.AppendLine($"ALTER TABLE {table.Table.Name}");
            var pk = table.Table.Indexes.FirstOrDefault(x => x.IsPrimaryKey && x.FieldsNames != null);
            sb.AppendLine($"ADD CONSTRAINT {pk.Name} PRIMARY KEY ({pk.FieldsNames});");
        }

        foreach (var table in transpiledTables.Where(x => x.Table.Indexes.Any(z => !z.IsPrimaryKey)))
        {
            sb.AppendLine($"ALTER TABLE {table.Table.Name}");
            sb.AppendJoin(",\n", table.Table.Indexes.Where(x => !x.IsPrimaryKey).Select(x =>
            $"ADD CONSTRAINT {x.Name} {(x.IsForeignKey ? $"FOREIGN KEY ({x.Field.Name}) REFERENCES {x.ReferencesTable} ({x.ReferencesField})" : "")}"));
            sb.AppendLine(";");
            sb.AppendLine();
        }

        return sb.ToString();
    }
};