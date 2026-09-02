namespace HMS.Shared.Abstractions.Domain;

/// <summary>
/// Base class untuk Value Object DDD. Dua value object dianggap sama
/// bila seluruh properti pembentuknya sama.
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(c => c?.GetHashCode() ?? 0)
            .Aggregate(17, (current, next) => current * 31 + next);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}