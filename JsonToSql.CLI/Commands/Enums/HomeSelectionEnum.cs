using System.ComponentModel;

namespace JsonToSql.CLI.Commands.Enums;

public enum HomeSelectionEnum
{
    [Description("Write JSON Manually")]
    WriteJson,

    [Description("Read JSON From File")]
    ReadFromFile,

    [Description("Show Output of the given JSON file")]
    ShowOutput
}