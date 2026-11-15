using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Genesis.Contracts;

namespace Genesis.UI;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private bool _refreshing;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => { await EnsureHostAsync(); await RefreshAsync(); _timer.Start(); };
        Closed += (_, _) => _timer.Stop();
        _timer.Tick += async (_, _) => await RefreshAsync();
    }

    private static async Task<Response> SendAsync(Request request)
    {
        using var pipe = new NamedPipeClientStream(".", Protocol.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(1500);
        using var writer = new StreamWriter(pipe, new UTF8Encoding(false), leaveOpen: true) { AutoFlush = true };
        using var reader = new StreamReader(pipe, Encoding.UTF8, leaveOpen: true);
        await writer.WriteLineAsync(JsonSerializer.Serialize(request, Protocol.Json));
        var reply = await reader.ReadLineAsync();
        return reply is null ? new(false, "Keine Antwort vom Host") : JsonSerializer.Deserialize<Response>(reply, Protocol.Json) ?? new(false, "Ungültige Antwort");
    }

    private static async Task EnsureHostAsync()
    {
        try { await SendAsync(new Request("status")); return; } catch { }
        var path = Path.Combine(AppContext.BaseDirectory, "Genesis.Host.exe");
        if (!File.Exists(path)) return;
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = false, CreateNoWindow = true });
        for (var i = 0; i < 15; i++)
        {
            await Task.Delay(200);
            try { await SendAsync(new Request("status")); return; } catch { }
        }
    }

    private async Task RefreshAsync()
    {
        if (_refreshing) return;
        _refreshing = true;
        try
        {
            var response = await SendAsync(new Request("status"));
            ConnectionText.Text = response.Success ? "● Host verbunden" : response.Message;
            if (response.Data is { } data)
            {
                DecisionsList.ItemsSource = data.Decisions.OrderByDescending(x => x.CreatedAt)
                    .Select(x => $"{x.CreatedAt.LocalDateTime:g}  {x.Text}").ToArray();
                JobsList.ItemsSource = data.Jobs.OrderByDescending(x => x.CreatedAt)
                    .Select(x => $"{x.Title}\n{x.State} – {x.Completed}/{x.Total}  ·  {x.Id[..8]}").ToArray();
            }
        }
        catch { ConnectionText.Text = "○ Host nicht erreichbar – Log prüfen"; }
        finally { _refreshing = false; }
    }

    private async void StartJob_Click(object sender, RoutedEventArgs e)
    {
        try { var result = await SendAsync(new Request("start-demo")); if (!result.Success) MessageBox.Show(result.Message); await RefreshAsync(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Verbindungsfehler"); }
    }

    private async void Remember_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DecisionInput.Text)) return;
        try
        {
            var result = await SendAsync(new Request("remember", DecisionInput.Text));
            if (result.Success) DecisionInput.Clear(); else MessageBox.Show(result.Message);
            await RefreshAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Verbindungsfehler"); }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await RefreshAsync();
}