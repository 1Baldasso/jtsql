namespace JsonToSql.CLI.Domain.Fields;

internal struct JsonShortDescription : IEquatable<JsonShortDescription>
{
    public string Key { get; set; }
    public object? Value { get; set; }

    public readonly bool Equals(JsonShortDescription other)
    {
        if (this.Key != other.Key)
            return false;
        if ((this.Value == null || other.Value == null) && this.Value != other.Value)
            return false;
        if (this.Value is IDictionary<string, object> dictionary && other.Value is IDictionary<string, object> oD)
            return dictionary.SequenceEqual(oD);
        if (this.Value is IEnumerable<JsonShortDescription> en && other.Value is IEnumerable<JsonShortDescription> oEn)
            return en.SequenceEqual(oEn);
        return this.Value?.Equals(other.Value) ?? true;
    }

    public override readonly bool Equals(object? obj) => obj is JsonShortDescription decription && Equals(decription);

    public static implicit operator JsonShortDescription((string, object?) tuple) => new JsonShortDescription
    {
        Key = tuple.Item1,
        Value = tuple.Item2,
    };

    public override readonly int GetHashCode() => base.GetHashCode();
}