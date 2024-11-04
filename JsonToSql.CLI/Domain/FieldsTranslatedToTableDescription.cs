using JsonToSql.CLI.Domain.Fields;
using JsonToSql.CLI.Domain.Table;

namespace JsonToSql.CLI.Domain;

internal struct FieldsTranslatedToTableDescription
{
    public ICollection<FieldRelationDefinition> Fields { get; set; }
    public DatabaseTableInfo Table { get; set; }
}