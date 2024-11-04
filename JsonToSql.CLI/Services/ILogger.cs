using JsonToSql.CLI.Commands.Settings;
using JsonToSql.CLI.Providers;
using Spectre.Console;

namespace JsonToSql.CLI.Services;

internal interface ILogger
{
    void LogTrace(string message);

    void LogError(string message);

    void LogWarning(string message);

    void LogInformation(string message);

    void LogWarning(string message, Exception exception);

    void LogError(Exception exception);
}

internal class Logger(IAnsiConsole console, ISettingsProvider<CommonSettings> settings) : ILogger
{
    private readonly IAnsiConsole _console = console;
    private readonly ISettingsProvider<CommonSettings> _settings = settings;

    public void LogError(string message)
    {
        _console.MarkupLine($"[red][[error]]: {message.EscapeMarkup()}[/]");
    }

    public void LogError(Exception exception)
    {
        _console.MarkupLine($"[red][[error]]: {exception.Message.EscapeMarkup()}[/]");
    }

    public void LogInformation(string message)
    {
        _console.MarkupLine($"[grey89][[info]]: {message.EscapeMarkup()}[/]");
    }

    public void LogTrace(string message)
    {
        if (_settings.Settings?.Verbose ?? false)
            _console.MarkupLine($"[grey][[trace]]: {message}[/]");
    }

    public void LogWarning(string message)
    {
        _console.MarkupLine($"[orange1][[warning]]: {message.EscapeMarkup()}[/]");
    }

    public void LogWarning(string message, Exception exception)
    {
        _console.MarkupLine($"[orange1][[warning]]: {message.EscapeMarkup()} - {exception.Message.EscapeMarkup()}[/]");
    }
}