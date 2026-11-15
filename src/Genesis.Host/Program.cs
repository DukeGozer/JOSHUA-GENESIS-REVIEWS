using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Genesis.Contracts;

namespace Genesis.Host;

internal static class Program
{
    private static readonly object Gate = new();
    private static readonly string DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JoshuaSpike5Genesis");
    private static readonly string StatePath = Path.Combine(DataDirectory, "state.json");
    private static readonly string LogPath = Path.Combine(DataDirectory, "host.log");
    private static Snapshot _state = new();

    private static async Task Main()
    {
        Directory.CreateDirectory(DataDirectory);
        using var singleton = new FileStream(Path.Combine(DataDirectory, "host.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        try
        {
            Load();
            _ = Task.Run(WorkLoop);
            while (true)
            {
                var server = new NamedPipeServerStream(Protocol.PipeName, PipeDirection.InOut, 10,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                try { await server.WaitForConnectionAsync(); _ = Task.Run(() => HandleClient(server)); }
                catch (Exception ex) { server.Dispose(); Log(ex.ToString()); await Task.Delay(300); }
            }
        }
        catch (Exception ex) { Log(ex.ToString()); }
    }

    private static async Task HandleClient(NamedPipeServerStream pipe)
    {
        using (pipe)
        using (var reader = new StreamReader(pipe, Encoding.UTF8, leaveOpen: true))
        using (var writer = new StreamWriter(pipe, new UTF8Encoding(false), leaveOpen: true) { AutoFlush = true })
        {
            try
            {
                var line = await reader.ReadLineAsync();
                if (line is null) return;
                var request = JsonSerializer.Deserialize<Request>(line, Protocol.Json);
                Response response;
                lock (Gate) response = Execute(request);
                await writer.WriteLineAsync(JsonSerializer.Serialize(response, Protocol.Json));
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                try { await writer.WriteLineAsync(JsonSerializer.Serialize(new Response(false, "Anfrage fehlgeschlagen. Details in host.log."), Protocol.Json)); }
                catch { /* client disconnected */ }
            }
        }
    }

    private static Response Execute(Request? request)
    {
        if (request is null) return new(false, "Leere Anfrage");
        switch (request.Command.ToLowerInvariant())
        {
            case "status": return new(true, "GENESIS Host erreichbar", Clone());
            case "remember":
                if (string.IsNullOrWhiteSpace(request.Text)) return new(false, "Bitte Beschlusstext angeben.");
                _state.Decisions.Add(new Decision(Guid.NewGuid().ToString("N"), request.Text.Trim(), DateTimeOffset.UtcNow));
                Save(); return new(true, "Beschluss dauerhaft gespeichert.", Clone());
            case "start-demo":
                var job = new Job(Guid.NewGuid().ToString("N"), "SPIKE-Testauftrag: 10 Verarbeitungsschritte", "queued", 0, 10, DateTimeOffset.UtcNow);
                _state.Jobs.Add(job); Save(); return new(true, "Testauftrag gestartet.", Clone());
            case "cancel":
                var index = _state.Jobs.FindIndex(x => x.Id == request.Id);
                if (index < 0) return new(false, "Auftrag nicht gefunden.");
                if (_state.Jobs[index].State is "done" or "canceled") return new(false, "Auftrag bereits beendet.");
                _state.Jobs[index] = _state.Jobs[index] with { State = "canceled" };
                Save(); return new(true, "Auftrag abgebrochen.", Clone());
            default: return new(false, "Unbekannter Befehl.");
        }
    }

    private static async Task WorkLoop()
    {
        while (true)
        {
            await Task.Delay(1000);
            try
            {
                lock (Gate)
                {
                    var index = _state.Jobs.FindIndex(x => x.State is "queued" or "running");
                    if (index < 0) continue;
                    var job = _state.Jobs[index];
                    var next = Math.Min(job.Total, job.Completed + 1);
                    _state.Jobs[index] = job with { State = next == job.Total ? "done" : "running", Completed = next };
                    Save();
                }
            }
            catch (Exception ex) { Log(ex.ToString()); }
        }
    }

    private static Snapshot Clone() => new()
    {
        Decisions = [.. _state.Decisions], Jobs = [.. _state.Jobs], UpdatedAt = _state.UpdatedAt
    };

    private static void Load()
    {
        if (!File.Exists(StatePath)) return;
        try { _state = JsonSerializer.Deserialize<Snapshot>(File.ReadAllText(StatePath), Protocol.Json) ?? new Snapshot(); }
        catch (Exception ex)
        {
            Log("Zustand nicht lesbar; keine Daten überschrieben: " + ex);
            throw; // fail closed; don't replace corrupted state with empty state
        }
    }

    private static void Save()
    {
        _state.UpdatedAt = DateTimeOffset.UtcNow;
        var temp = StatePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(_state, Protocol.Json), new UTF8Encoding(false));
        File.Move(temp, StatePath, overwrite: true);
    }

    private static void Log(string line)
    {
        try { lock (Gate) File.AppendAllText(LogPath, $"{DateTimeOffset.UtcNow:u} {line}{Environment.NewLine}"); }
        catch { }
    }
}