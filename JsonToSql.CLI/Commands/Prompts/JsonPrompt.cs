using JsonToSql.CLI.Extensions;
using Spectre.Console;

namespace JsonToSql.CLI.Commands.Prompts;

public static class JsonPrompt
{
    private static TextPrompt<string> Prompt { get; set; } = new TextPrompt<string>(">") { AllowEmpty = true, PromptStyle = Style.Parse("lightyellow3") };

    public static string GetJson()
    {
        var sw = new StringWriter();
        while (true)
        {
            var line = AnsiConsole.Prompt(Prompt);
            if (string.IsNullOrEmpty(line))
                break;
            sw.WriteLine(line);
        }
        var content = sw.ToString();
        if (!content.IsJson() && !content.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine("[red]The given content is not a valid JSON.[/]");
            return GetJson();
        }
        return content;
    }
}