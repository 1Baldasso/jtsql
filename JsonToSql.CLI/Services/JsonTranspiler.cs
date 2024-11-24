using JsonToSql.CLI.Commands.Settings;
using JsonToSql.CLI.Domain.Fields;
using JsonToSql.CLI.Extensions;
using JsonToSql.CLI.Providers;
using Spectre.Console;
using System.Text.Json;

namespace JsonToSql.CLI.Services;

internal interface IJsonTranspiler
{
    Task<string> Transpile(string json, CancellationToken cancellationToken = default);
}

internal class JsonTranspiler(ISqlInitialTranspiler initialTranspiler, ISqlWriter sqlWriter, IAnsiConsole console, ISettingsProvider<CommonSettings> settings, ILogger logger, ISqlIndexTanspiler indexTranspiler) : IJsonTranspiler
{
    private readonly ISqlInitialTranspiler _initialTranspiler = initialTranspiler;
    private readonly ISqlIndexTanspiler _indexTranspiler = indexTranspiler;
    private readonly ISqlWriter _sqlWriter = sqlWriter;
    private readonly ILogger _logger = logger;

    private readonly JsonSerializerOptions _traceSerializer = new JsonSerializerOptions
    {
        WriteIndented = false,
        AllowTrailingCommas = false,
    };

    public async Task<string> Transpile(string json, CancellationToken cancellationToken = default)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
        };
        var dic = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, options) ?? throw new JsonException("Could not parse JSON Objec");
        var tableCandidates = GetTableCandidates(dic);
        var transpiledChildren = _initialTranspiler.TranspileChildrenData(dic, tableCandidates);
        var transpiledRoot = _initialTranspiler.TranspileRootData(dic, tableCandidates.SelectMany(x => x).ToArray());
        _indexTranspiler.UpdateIndexes(dic, transpiledRoot, transpiledChildren);
        var finalResult = await _sqlWriter.WriteResult([transpiledRoot, .. transpiledChildren], cancellationToken);
        return finalResult;
    }

    private string[][] GetTableCandidates(Dictionary<string, JsonElement> dic)
    {
        var set = new Dictionary<string, JsonShortDescription>();
        foreach (var (key, element) in dic)
        {
            set[key] = element.GetJsonElementInfo();
        }
        var grouped = new Dictionary<int, List<string>>();
        foreach (var (key, element, index) in set.Select((x, index) => (x.Key, x.Value, index)))
        {
            if (element.Value is null)
            {
                continue;
            }
            var othersToCompare = set.Where(x => x.Key != key && x.Value.Value != null).ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in othersToCompare)
            {
                if (element.Key == "array")
                {
                    _logger.LogTrace($"Element [white]{item.Key}[/] is array, checking inner objects and comparing to other objects and arrays");
                    if (item.Value.Value is ICollection<JsonShortDescription> itemValue && element.Value is ICollection<JsonShortDescription> elementValue)
                    {
                        var firstItemValue = itemValue.FirstOrDefault();
                        var firstElementValue = elementValue.FirstOrDefault();
                        if (firstItemValue.Key == firstElementValue.Key)
                        {
                            if (firstItemValue.Key != firstElementValue.Key)
                                continue;
                            if (firstItemValue.Key != "array" && firstItemValue.Key != "object")
                            {
                                if (grouped.Any(x => x.Value.Contains(item.Key) || x.Value.Contains(key)))
                                {
                                    _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table, matching existing values [white]{JsonSerializer.Serialize(grouped.Values, _traceSerializer).EscapeMarkup()}[/]");
                                    var k = grouped.FirstOrDefault(x => x.Value.Contains(item.Key) || x.Value.Contains(key)).Key;
                                    grouped[k].Add(item.Key);
                                }
                                else
                                {
                                    _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table");
                                    grouped[index] = [item.Key];
                                }
                                continue;
                            }
                            if (firstItemValue.Value is IDictionary<string, JsonShortDescription> fivdic
                                && firstElementValue.Value is IDictionary<string, JsonShortDescription> fevdic)
                            {
                                if (fivdic.Keys.SequenceEqual(fevdic.Keys))
                                {
                                    if (grouped.Any(x => x.Value.Contains(item.Key) || x.Value.Contains(key)))
                                    {
                                        _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table, matching existing values [white]{JsonSerializer.Serialize(grouped.Values, _traceSerializer).EscapeMarkup()}[/]");
                                        var k = grouped.FirstOrDefault(x => x.Value.Contains(item.Key) || x.Value.Contains(key)).Key;
                                        grouped[k].Add(item.Key);
                                    }
                                    else
                                    {
                                        _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table");
                                        grouped[index] = [item.Key];
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                    else if (item.Value is JsonShortDescription ivjs && ivjs.Key == "object"
                        && element.Value is ICollection<JsonShortDescription> elVal
                        && item.Value.Value is IDictionary<string, JsonShortDescription> fevdic
                        && elVal.FirstOrDefault().Value is IDictionary<string, JsonShortDescription> fivdic)
                    {
                        _logger.LogTrace($"Element [white]{item.Key}[/] is an object, checking other objects and arrays");

                        if (fivdic.Keys.SequenceEqual(fevdic.Keys))
                        {
                            if (grouped.Any(x => x.Value.Contains(item.Key) || x.Value.Contains(key)))
                            {
                                _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table, matching existing values [white]{JsonSerializer.Serialize(grouped.Values, _traceSerializer).EscapeMarkup()}[/]");
                                var k = grouped.FirstOrDefault(x => x.Value.Contains(item.Key) || x.Value.Contains(key)).Key;
                                grouped[k].Add(item.Key);
                            }
                            else
                            {
                                _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table");
                                grouped[index] = [item.Key];
                            }
                            continue;
                        }
                    }
                }
                else if (element.Key == "object")
                {
                    _logger.LogTrace($"Element [white]{item.Key}[/] is an object, checking other objects and arrays");
                    if (element.Value is IDictionary<string, JsonShortDescription> fivdic
                                && item.Value.Value is IDictionary<string, JsonShortDescription> fevdic)
                    {
                        if (fivdic.Keys.SequenceEqual(fevdic.Keys))
                        {
                            if (grouped.Any(x => x.Value.Contains(item.Key) || x.Value.Contains(key)))
                            {
                                _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table, matching existing values [white]{JsonSerializer.Serialize(grouped.Values, _traceSerializer).EscapeMarkup()}[/]");
                                var k = grouped.FirstOrDefault(x => x.Value.Contains(item.Key) || x.Value.Contains(key)).Key;
                                grouped[k].Add(item.Key);
                            }
                            else
                            {
                                _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table");
                                grouped[index] = [item.Key];
                            }
                            continue;
                        }
                    }
                    else if (element.Value is IDictionary<string, JsonShortDescription> ofivdic
                                && item.Value.Value is ICollection<JsonShortDescription> ofevdic)
                    {
                        if (ofevdic.FirstOrDefault().Value is IDictionary<string, JsonShortDescription> js)
                        {
                            var keys = js.Select(x => x.Key).ToArray();
                            if (ofivdic.Keys.SequenceEqual(keys))
                            {
                                if (grouped.Any(x => x.Value.Contains(item.Key) || x.Value.Contains(key)))
                                {
                                    _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table, matching existing values [white]{JsonSerializer.Serialize(grouped.Values, _traceSerializer).EscapeMarkup()}[/]");
                                    var k = grouped.FirstOrDefault(x => x.Value.Contains(item.Key) || x.Value.Contains(key)).Key;
                                    grouped[k].Add(item.Key);
                                }
                                else
                                {
                                    _logger.LogTrace($"Found [white]{item.Key}[/] values to be possibly a table");
                                    grouped[index] = [item.Key];
                                }
                                continue;
                            }
                        }
                    }
                }
            }
        }
        var singles = set.Where(x => x.Value.Value != null && !grouped.SelectMany(x => x.Value).Contains(x.Key)).ToArray();
        var result = grouped.Select(x => x.Value.ToArray()).Distinct().ToArray().Concat(singles.Select(x => (string[])[x.Key])).ToArray().ToArray();
        var found = result.SelectMany(x => x).ToArray();
        _logger.LogTrace($"Found {found.Length} table candidates: [white]{JsonSerializer.Serialize(found, _traceSerializer).EscapeMarkup()}[/]");
        return result;
    }
}