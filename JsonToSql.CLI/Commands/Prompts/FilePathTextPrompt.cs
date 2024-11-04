using JsonToSql.CLI.Commands.Enums;
using Spectre.Console;

namespace JsonToSql.CLI.Commands.Prompts;

internal static class FilePathTextPrompt
{
    public static readonly TextPrompt<string> Prompt;

    static FilePathTextPrompt()
    {
        Prompt = new TextPrompt<string>("[yellow]Write down the JSON file path.[/]")
        {
            Validator = (path) => Path.Exists(path) ? ValidationResult.Success() : ValidationResult.Error(),
            ValidationErrorMessage = "[red] The given path is not a valid PATH[/]",
            ShowDefaultValue = true,
        };
        Prompt.DefaultValue("./teste.json");
    }
}