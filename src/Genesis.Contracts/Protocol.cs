using System.Text.Json;

namespace Genesis.Contracts;

public static class Protocol
{
    public const string PipeName = "joshua-spike-5-genesis-v1";
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
}

public sealed record Request(string Command, string? Text = null, string? Id = null);
public sealed record Response(bool Success, string Message, Snapshot? Data = null);
public sealed record Decision(string Id, string Text, DateTimeOffset CreatedAt);
public sealed record Job(string Id, string Title, string State, int Completed, int Total, DateTimeOffset CreatedAt, string? Error = null);
public sealed class Snapshot
{
    public List<Decision> Decisions { get; set; } = [];
    public List<Job> Jobs { get; set; } = [];
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}