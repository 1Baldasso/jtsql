using Spectre.Console.Cli;

namespace JsonToSql.CLI.Providers;

internal interface ISettingsProvider<T>
    where T : CommandSettings
{
    public T? Settings { get; set; }
}

internal class SettingsProvider<T> : ISettingsProvider<T>
    where T : CommandSettings
{
    private T? _settings = null;
    T? ISettingsProvider<T>.Settings { get => _settings; set => _settings = _settings != null ? throw new InvalidOperationException("Cant set the settings again") : value; }
}