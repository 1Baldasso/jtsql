using JsonToSql.CLI.Domain;
using JsonToSql.CLI.Domain.Fields;
using JsonToSql.CLI.Services;
using System.Text.Json;

namespace JsonToSql.CLI.Extensions;

internal static class JsonParseExtension
{
    internal static JsonShortDescription GetJsonElementInfo(this JsonElement element)
    {
        var serializerSettings = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
        };

        return element.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.Null => ("unknown", (object?)null),
            JsonValueKind.False or JsonValueKind.True => ("boolean", null),
            JsonValueKind.Object => ("object", JsonSerializer.Deserialize<IDictionary<string, JsonElement>>(element.GetRawText(), serializerSettings)?
                .ToDictionary(x => x.Key, x => x.Value.GetJsonElementInfo())),
            JsonValueKind.Array => ("array", JsonSerializer.Deserialize<IEnumerable<JsonElement>>(element.GetRawText(), serializerSettings)?
                .Select(x => x.GetJsonElementInfo()).ToArray()),
            JsonValueKind.String => DateOnly.TryParse(element.GetRawText(), out var _)
                ? ("dateonly", null)
                : DateTime.TryParse(element.GetRawText(), out var _)
                ? ("datetime", null)
                : element.TryGetGuid(out var _)
                ? ("guid", null)
                : ("string", null),
            JsonValueKind.Number => element.GetRawText().Contains('.') ? ("decimal", null) : element.TryGetInt32(out var _) ? ("integer", null) : throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }

    internal static DatabaseFieldInfo GetFieldInfo(this JsonElement element, string name, ILogger logger)
    {
        logger.LogTrace($"Trying to convert the field [white]{name}[/] with value [white]{element.GetRawText()}[/]");
        (string clr, string sql) = element.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.Null => ("object", "NULL"),
            JsonValueKind.False or JsonValueKind.True => ("boolean", "bit"),
            JsonValueKind.Object => ("object", "table"),
            JsonValueKind.Array => ("array", "table"),
            JsonValueKind.String =>
                DateTime.TryParse(element.GetString(), out var date)
                ? date.TimeOfDay.Negate() == date.TimeOfDay ?
                    ("dateonly", "date")
                    : date.Kind == DateTimeKind.Unspecified
                    ? ("datetime", "timestamp")
                    : ("datetime", "timestamptz")
                : element.TryGetGuid(out var _)
                ? ("guid", "uniqueidentifier")
                : ("string", "varchar"),
            JsonValueKind.Number => element.GetRawText().Contains('.')
                ? ("decimal", "decimal")
                : element.TryGetInt32(out var _)
                ? ("int", "integer")
                : throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
        logger.LogTrace($"Field {name} found to be a [white]{clr}[/], and will be considered [white]{sql}[/] on the database");
        return new DatabaseFieldInfo
        {
            ClrType = clr,
            DatabaseType = sql,
            Name = name,
            IsNullable = true,
            IsAutoIncrement = false,
        };
    }
}