using JsonToSql.CLI.Commands.Settings;
using JsonToSql.CLI.Providers;
using Spectre.Console.Cli;

namespace JsonToSql.CLI.Interceptors;

internal class SettingsCommandInterceptor(ISettingsProvider<CommonSettings> settingsProvider) : ICommandInterceptor
{
    private readonly ISettingsProvider<CommonSettings> _settingsProvider = settingsProvider;

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        _settingsProvider.Settings = settings as CommonSettings;
    }
}