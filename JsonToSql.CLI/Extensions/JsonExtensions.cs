using System.Text.Json;

namespace JsonToSql.CLI.Extensions;

internal static class JsonExtensions
{
    internal static bool IsJson(this string content)
    {
        try
        {
            JsonDocument.Parse(content, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                MaxDepth = 14,
            });
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}