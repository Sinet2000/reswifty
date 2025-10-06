namespace Reswifty.API.Domain.Common;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public byte[]? RowVersion { get; protected set; }

    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    public bool Equals(BaseEntity? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is BaseEntity e && Equals(e);
    public override int GetHashCode() => Id.GetHashCode();
}