using JsonToSql.CLI.Commands.Enums;
using JsonToSql.CLI.Extensions;
using Spectre.Console;

namespace JsonToSql.CLI.Commands.Prompts;

public static class HomeSelectPrompt
{
    public static readonly MultiSelectionPrompt<HomeSelectionEnum> Prompt;

    static HomeSelectPrompt()
    {
        Prompt = new MultiSelectionPrompt<HomeSelectionEnum>
        {
            Title = "What do you want to do?",
            Converter = (t) => t.GetDescription(),
        };
        Prompt.AddChoices(Enum.GetValues<HomeSelectionEnum>());
    }
}