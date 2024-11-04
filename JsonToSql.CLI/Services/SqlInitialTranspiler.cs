using JsonToSql.CLI.Commands.Settings;
using JsonToSql.CLI.Domain;
using JsonToSql.CLI.Domain.Fields;
using JsonToSql.CLI.Domain.Table;
using JsonToSql.CLI.Extensions;
using JsonToSql.CLI.Providers;
using Spectre.Console;
using System.Text.Json;

namespace JsonToSql.CLI.Services;

internal interface ISqlInitialTranspiler
{
    ICollection<FieldsTranslatedToTableDescription> TranspileChildrenData(IDictionary<string, JsonElement> jsonInfo, IEnumerable<string[]> tableCandidates);

    FieldsTranslatedToTableDescription TranspileRootData(IDictionary<string, JsonElement> dic, ICollection<string> alreadTranspiledNames);
}

internal class SqlInitialTranspiler(ISettingsProvider<CommonSettings> settingsProvider, ILogger logger) : ISqlInitialTranspiler
{
    private readonly ISettingsProvider<CommonSettings> _settings = settingsProvider;
    private readonly ILogger _logger = logger;

    public ICollection<FieldsTranslatedToTableDescription> TranspileChildrenData(IDictionary<string, JsonElement> jsonInfo, IEnumerable<string[]> tableCandidates)
    {
        return tableCandidates.Select(x => new FieldsTranslatedToTableDescription
        {
            Table = GetTableInfo(jsonInfo[x[0]], x[0]),
            Fields = x.Select(z => new FieldRelationDefinition
            {
                Name = z,
                IsOneToOne = jsonInfo[z].ValueKind == JsonValueKind.Object && !CheckOneToMany(jsonInfo[z]),
                IsOneToMany = (jsonInfo[z].ValueKind == JsonValueKind.Array && !CheckManyToMany(jsonInfo[z]))
                    || (jsonInfo[z].ValueKind == JsonValueKind.Object && CheckOneToMany(jsonInfo[z])),
                IsManyToMany = jsonInfo[z].ValueKind == JsonValueKind.Array && CheckManyToMany(jsonInfo[z]),
                OneSide = jsonInfo[z].ValueKind == JsonValueKind.Array && !CheckManyToMany(jsonInfo[z])
                    ? _settings.Settings?.RootTableName
                    : jsonInfo[z].ValueKind == JsonValueKind.Object && CheckOneToMany(jsonInfo[z])
                    ? z
                    : null,
            }).ToList(),
        }).ToList();
    }

    private bool CheckOneToMany(JsonElement jsonElement)
    {
        return jsonElement.Deserialize<IDictionary<string, JsonElement>>()?
            .Any(x => x.Key.Contains(_settings.Settings?.RootTableName!) && x.Value.ValueKind == JsonValueKind.Array) ?? false;
    }

    private bool CheckManyToMany(JsonElement jsonElement)
    {
        return jsonElement.Deserialize<ICollection<IDictionary<string, JsonElement>>>()?.FirstOrDefault()?
            .Any(x => x.Key.Contains(_settings.Settings?.RootTableName!) && x.Value.ValueKind == JsonValueKind.Array) ?? false;
    }

    private DatabaseTableInfo GetTableInfo(JsonElement jsonElement, string tableName)
    {
        var item = jsonElement.ValueKind == JsonValueKind.Object
            ? jsonElement.Deserialize<IDictionary<string, JsonElement>>()
            : jsonElement.Deserialize<ICollection<IDictionary<string, JsonElement>>>()?.FirstOrDefault();
        var itemId = new DatabaseFieldInfo
        {
            Name = tableName + "_id",
            ClrType = _settings.Settings.ClrType,
            DatabaseType = _settings.Settings.KeyType,
            IsAutoIncrement = _settings.Settings?.AutoIncrement ?? false,
            IsNullable = false,
            IsPk = true,
        };
        if (itemId.IsAutoIncrement && itemId.DatabaseType != "uuid")
        {
            itemId.DatabaseType = "SERIAL";
        }
        return new DatabaseTableInfo
        {
            Name = tableName,
            Fields = [.. item?.Select(x => x.Value.GetFieldInfo(x.Key, _logger)).Where(x => x.DatabaseType != "table") ?? [], itemId],
            Indexes = [new DatabaseIndex
            {
                Field = itemId,
                IsForeignKey = false,
                IsPrimaryKey = true,
                Name = $"PK_{_settings.Settings.RootTableName}_{itemId.Name}"
            }]
        };
    }

    public FieldsTranslatedToTableDescription TranspileRootData(IDictionary<string, JsonElement> dic, ICollection<string> alreadTranspiledNames)
    {
        var toCheck = dic.Where(x => !alreadTranspiledNames.Contains(x.Key));
        var rootId = new DatabaseFieldInfo
        {
            Name = _settings.Settings?.RootTableName! + "_id",
            ClrType = _settings.Settings?.ClrType!,
            DatabaseType = _settings.Settings?.KeyType!,
            IsAutoIncrement = _settings.Settings?.AutoIncrement ?? false,
            IsNullable = false,
            IsPk = true,
        };
        if (rootId.IsAutoIncrement && rootId.DatabaseType != "uuid")
        {
            rootId.DatabaseType = "SERIAL";
        }
        return new FieldsTranslatedToTableDescription
        {
            Table = new DatabaseTableInfo
            {
                Name = _settings.Settings?.RootTableName!,
                Fields = [.. toCheck.Select(x => x.Value.GetFieldInfo(x.Key, _logger)), rootId],
                Indexes = [new DatabaseIndex
                {
                    Field = rootId,
                    IsForeignKey = false,
                    IsPrimaryKey = true,
                    Name = $"PK_{_settings.Settings.RootTableName}_{rootId.Name}"
                }],
            },
            Fields = [new FieldRelationDefinition
            {
                Name = "root",
                IsOneToMany = false,
                IsOneToOne = false,
            }],
        };
    }
}