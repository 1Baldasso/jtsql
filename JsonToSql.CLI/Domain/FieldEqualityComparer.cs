using JsonToSql.CLI.Domain.Fields;
using System.Diagnostics.CodeAnalysis;

namespace JsonToSql.CLI.Domain;

internal class FieldEqualityComparer : IEqualityComparer<ICollection<JsonShortDescription>>
{
    public bool Equals(ICollection<JsonShortDescription> x, ICollection<JsonShortDescription> y)
    {
        return x.SequenceEqual(y);
    }

    public int GetHashCode([DisallowNull] ICollection<JsonShortDescription> obj)
    {
        return obj.GetHashCode();
    }
}