using System.Text;
using System.Text.Json;

namespace OutfitPlanner.Application.Common;

/// <summary>
/// Helper class for cursor-based pagination
/// </summary>
public static class CursorPagination
{
    /// <summary>
    /// Creates a cursor from CreatedAt and Id
    /// </summary>
    public static string CreateCursor(DateTimeOffset createdAt, Guid id)
    {
        var cursorData = new CursorData
        {
            CreatedAt = createdAt,
            Id = id
        };
        var json = JsonSerializer.Serialize(cursorData);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static string CreateTrendingCursor(decimal score, Guid id)
    {
        var cursorData = new TrendingCursorData
        {
            Score = score,
            Id = id
        };
        var json = JsonSerializer.Serialize(cursorData);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>
    /// Decodes a cursor string back to CreatedAt and Id
    /// </summary>
    public static CursorData? DecodeCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
            return null;

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return JsonSerializer.Deserialize<CursorData>(json);
        }
        catch
        {
            return null;
        }
    }

    public static TrendingCursorData? DecodeTrendingCursor(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
            return null;

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return JsonSerializer.Deserialize<TrendingCursorData>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Response model for cursor-based paginated results
    /// </summary>
    public class CursorPagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public string? NextCursor { get; set; }
        public bool HasMore { get; set; }
        public int PageSize { get; set; }
    }
}

/// <summary>
/// Data stored in a cursor
/// </summary>
public class CursorData
{
    public DateTimeOffset CreatedAt { get; set; }
    public Guid Id { get; set; }
}

public class TrendingCursorData
{
    public decimal Score { get; set; }
    public Guid Id { get; set; }
}

