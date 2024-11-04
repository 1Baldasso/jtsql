namespace JsonToSql.CLI.Commands.Settings.Descriptions;

internal static class GlobalDescriptions
{
    internal const string PATH = "The path of the JSON file containing the structure to translate.";
    internal const string OUTPUT = "The path in which the translated SQL DDL query will be written to.";
    internal const string CONSOLE_OUTPUT = "Outputs the translated content into the console.";
    internal const string INTERACTIVE = "Initialize an interactive version of the program where you can write your JSON manually [red](beta)[/]";
    internal const string ROOT_TABLE_NAME = "Defines the table name of the transpiled data found in the root JSON object.\n\r\t\t\t[[root-name]]    The default value is the json FileName or [yellow]'root'[/] when no file is provided";
    internal const string DESCRIBE = "Describe what has been done to get to this result commented on the sql file and on screen if console-output selected";
    internal const string VERBOSE = "Logs every operation status to the console.";
    internal const string KEY_TYPE = "Defines the identifier type. [yellow]Valid values:[/] [grey]uuid | integer | int | varchar[/]";
    internal const string COPY_OUTPUT = "Copies the output into the clipboard";
    internal const string AUTO_INCREMENT = "Defines if the key auto-increments";
}