using JsonToSql.CLI.Commands.Settings;
using JsonToSql.CLI.Providers;

namespace JsonToSql.CLI.Extensions;

internal static class SettingsProviderExtension
{
    internal static string GetRootTableName(this ISettingsProvider<CommonSettings> settings)
    {
        return settings.Settings?.RootTableName ?? "root";
    }
}