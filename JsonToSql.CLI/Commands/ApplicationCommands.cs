using JsonToSql.CLI.Commands.Prompts;
using JsonToSql.CLI.Commands.Settings;
using JsonToSql.CLI.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JsonToSql.CLI.Commands;

internal sealed class ApplicationCommands(IJsonTranspiler transpiler) : AsyncCommand<CommonSettings>
{
    private readonly IJsonTranspiler _transpiler = transpiler;

    public override async Task<int> ExecuteAsync(CommandContext context, CommonSettings settings)
    {
        var json = "";
        if (settings.Path is not null)
        {
            json = await File.ReadAllTextAsync(settings.Path);
            if (settings.RootTableName is null)
            {
                settings.RootTableName = new string(System.IO.Path.GetFileNameWithoutExtension(settings.Path).Where(char.IsAsciiLetterOrDigit).ToArray());
            }
        }
        else
        {
            var directory = Directory.GetParent(typeof(ApplicationCommands).Assembly.Location);
            json = await File.ReadAllTextAsync(Path.Combine(directory.FullName, "teste.json"));
        }
        if (settings.Interactive)
        {
            AnsiConsole.MarkupLine("[blue]Welcome to JsonToSql! A tool which aims to create a simple database which can hold the given JSON output.[/]");
            var option = AnsiConsole.Prompt(HomeSelectPrompt.Prompt);
            if (option.Contains(Enums.HomeSelectionEnum.WriteJson))
            {
                AnsiConsole.MarkupLine("[cyan]To start, write below your desired JSON output.[/]");
                AnsiConsole.MarkupLine("[cyan]You must double-tap enter to finish the input[/]");
                json = JsonPrompt.GetJson();
            }
            if (option.Contains(Enums.HomeSelectionEnum.ShowOutput))
            {
                settings.ConsoleOutput = true;
            }
            if (option.Contains(Enums.HomeSelectionEnum.ReadFromFile))
            {
                var path = AnsiConsole.Prompt(FilePathTextPrompt.Prompt);
                settings.Path = path;
            }
        }
        if (settings.RootTableName is null)
        {
            settings.RootTableName = "root";
        }
        settings.RootTableName = new string(settings.RootTableName.Where(char.IsAsciiLetterOrDigit).ToArray());
        var transpiled = await _transpiler.Transpile(json);
        if (settings.ConsoleOutput)
        {
            AnsiConsole.Write(transpiled);
        }
        if (settings.OutputPath is not null)
        {
            var dicPath = Path.GetDirectoryName(settings.OutputPath);
            if (!Directory.Exists(dicPath))
            {
                Directory.CreateDirectory(dicPath);
            }
            //if(!File.Exists())
            await File.WriteAllTextAsync(settings.OutputPath, transpiled);
        }
        if (settings.CopyToClipboard)
        {
            TextCopy.ClipboardService.SetText(transpiled);
        }

        return 0;
    }
}