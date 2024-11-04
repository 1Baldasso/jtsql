using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using JsonToSql.CLI.Commands.Settings.Descriptions;
using JsonToSql.CLI.Extensions;

namespace JsonToSql.CLI.Commands.Settings;

internal sealed class CommonSettings : CommandSettings
{
    [Description(GlobalDescriptions.PATH)]
    [CommandArgument(0, "[path]")]
    public string? Path { get; set; }

    [Description(GlobalDescriptions.ROOT_TABLE_NAME)]
    [CommandOption("-n|--root-name")]
    public string? RootTableName { get; set; }

    [Description(GlobalDescriptions.OUTPUT)]
    [CommandOption("-o|--output")]
    public string? OutputPath { get; set; }

    [Description(GlobalDescriptions.KEY_TYPE)]
    [CommandOption("-k|--key")]
    [DefaultValue("uuid")]
    public string? KeyType { get; set; }

    [Description(GlobalDescriptions.CONSOLE_OUTPUT)]
    [CommandOption("-s|--output-console")]
    public bool ConsoleOutput { get; set; }

    [Description(GlobalDescriptions.INTERACTIVE)]
    [CommandOption("-i|--interactive")]
    public bool Interactive { get; set; }

    [Description(GlobalDescriptions.DESCRIBE)]
    [CommandOption("-d|--describe")]
    public bool Describe { get; set; }

    [Description(GlobalDescriptions.VERBOSE)]
    [CommandOption("-v|--verbose")]
    public bool Verbose { get; set; }

    [Description(GlobalDescriptions.COPY_OUTPUT)]
    [CommandOption("-c|--copy-output")]
    public bool CopyToClipboard { get; set; }

    [Description(GlobalDescriptions.AUTO_INCREMENT)]
    [CommandOption("-a|--auto-increment")]
    public bool AutoIncrement { get; set; }

    public string? ClrType => this.KeyType == null ? null : this.KeyType.Contains("int", StringComparison.OrdinalIgnoreCase) ? "int" : this.KeyType.Equals("uuid", StringComparison.OrdinalIgnoreCase) ? "guid" : "string";

    public override ValidationResult Validate()
    {
        if (!new[] { "uuid", "integer", "int", "varchar" }.Any(x => x.Equals(KeyType, StringComparison.OrdinalIgnoreCase)))
        {
            return ValidationResult.Error("The given key type is not supported");
        }
        if (KeyType.Equals("varchar", StringComparison.OrdinalIgnoreCase) && AutoIncrement)
        {
            return ValidationResult.Error("Auto-increment is not supported for text keys");
        }
        if (Path == null)
        {
            return ValidationResult.Success();
        }

        if (!System.IO.Path.Exists(Path))
        {
            return ValidationResult.Error("There was no file in the given path");
        }
        var json = File.ReadAllText(Path);
        return json.IsJson() ? ValidationResult.Success() : ValidationResult.Error("The content is not valid json");
    }
}