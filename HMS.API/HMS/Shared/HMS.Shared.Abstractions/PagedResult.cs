namespace HMS.Shared.Abstractions;

/// <summary>
/// Wrapper untuk hasil query ber-paginasi.
/// </summary>
public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageIndex > 1;
    public bool HasNext => PageIndex < TotalPages;
}