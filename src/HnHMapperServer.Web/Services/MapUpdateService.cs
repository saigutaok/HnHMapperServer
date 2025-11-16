using HnHMapperServer.Web.Models;
using System.Text.Json;

namespace HnHMapperServer.Web.Services;

public class MapUpdateService : IAsyncDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MapUpdateService> _logger;
    private HttpClient? _sseClient;
    private Stream? _sseStream;
    private CancellationTokenSource? _cts;
    private Task? _readTask;
    private int _reconnectAttempts = 0;
    private const int MaxReconnectDelay = 5000; // 5 seconds max
    private bool _isDisposed = false;

    public event Action<List<TileUpdate>>? OnTileUpdate;
    public event Action<MapMerge>? OnMapMerge;
    public event Action<MapInfoModel>? OnMapUpdated;
    public event Action<int>? OnMapDeleted;
    public event Action<int, int>? OnMapRevision; // (mapId, revision)

    public MapUpdateService(IHttpClientFactory httpClientFactory, ILogger<MapUpdateService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task ConnectAsync()
    {
        // Don't reconnect if already disposed
        if (_isDisposed) return;

        try
        {
            // Clean up previous connection if exists
            await CleanupConnectionAsync();

            _cts = new CancellationTokenSource();
            _sseClient = _httpClientFactory.CreateClient("API");

            // Set timeout to infinite for SSE
            _sseClient.Timeout = Timeout.InfiniteTimeSpan;

            var request = new HttpRequestMessage(HttpMethod.Get, "/map/updates");
            request.Headers.Add("Accept", "text/event-stream");

            var response = await _sseClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
            response.EnsureSuccessStatusCode();

            _sseStream = await response.Content.ReadAsStreamAsync(_cts.Token);

            // Reset reconnect attempts on successful connection
            _reconnectAttempts = 0;

            // Start reading SSE messages in background with auto-reconnect
            _readTask = Task.Run(() => ReadSseStreamWithReconnectAsync(_cts.Token), _cts.Token);

            _logger.LogInformation("Connected to SSE updates stream");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to SSE stream");
            // Schedule reconnect on connection failure
            _ = ScheduleReconnectAsync();
        }
    }

    private async Task CleanupConnectionAsync()
    {
        _sseStream?.Dispose();
        _sseStream = null;
        _sseClient?.Dispose();
        _sseClient = null;
    }

    private async Task ReadSseStreamWithReconnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ReadSseStreamAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("SSE stream reading cancelled");
        }
        catch (IOException ex)
        {
            // IOException is common during network/app restarts or transient disconnects - log at Debug
            _logger.LogDebug(ex, "SSE stream disconnected (network/transport error) - will reconnect");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // 401 during reconnect (expected if cookie expired/missing) - log at Info
            _logger.LogInformation("SSE reconnect got 401 Unauthorized - will retry with backoff");
        }
        catch (Exception ex)
        {
            // Unexpected errors - keep as Error
            _logger.LogError(ex, "Unexpected error reading SSE stream");
        }

        // Stream ended or errored - schedule reconnect if not cancelled
        if (!cancellationToken.IsCancellationRequested && !_isDisposed)
        {
            _logger.LogInformation("SSE stream ended - scheduling reconnect");
            _ = ScheduleReconnectAsync();
        }
    }

    private async Task ReadSseStreamAsync(CancellationToken cancellationToken)
    {
        if (_sseStream == null) return;

        using var reader = new StreamReader(_sseStream);
        string? eventType = null;
        var dataLines = new List<string>();

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            if (line == null)
            {
                // End of stream
                break;
            }

            if (string.IsNullOrEmpty(line))
            {
                // Empty line signals end of event
                if (dataLines.Count > 0)
                {
                    ProcessSseEvent(eventType, string.Join("\n", dataLines));
                    dataLines.Clear();
                    eventType = null;
                }
                continue;
            }

            if (line.StartsWith("event:"))
            {
                eventType = line.Substring(6).Trim();
            }
            else if (line.StartsWith("data:"))
            {
                dataLines.Add(line.Substring(5).Trim());
            }
        }
    }

    private async Task ScheduleReconnectAsync()
    {
        if (_isDisposed) return;

        _reconnectAttempts++;

        // Calculate backoff delay: 1s, 2s, 5s (capped)
        var baseDelay = _reconnectAttempts switch
        {
            1 => 1000,
            2 => 2000,
            _ => MaxReconnectDelay
        };

        // Add jitter (±20%)
        var jitter = Random.Shared.Next(-200, 200);
        var delay = Math.Max(500, baseDelay + jitter);

        _logger.LogInformation("Scheduling SSE reconnect attempt {Attempt} in {Delay}ms", _reconnectAttempts, delay);

        await Task.Delay(delay);

        if (!_isDisposed)
        {
            await ConnectAsync();
        }
    }

    private void ProcessSseEvent(string? eventType, string data)
    {
        try
        {
            if (eventType == "merge")
            {
                // Map merge event
                var merge = JsonSerializer.Deserialize<MapMerge>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (merge != null)
                {
                    _logger.LogInformation("Received map merge event: {From} -> {To}", merge.From, merge.To);
                    OnMapMerge?.Invoke(merge);
                }
            }
            else if (eventType == "mapUpdate")
            {
                // Map metadata update event (rename, hidden, priority change)
                var mapUpdate = JsonSerializer.Deserialize<MapUpdateDto>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (mapUpdate != null)
                {
                    _logger.LogInformation("Received map update event: {Id} - {Name}", mapUpdate.Id, mapUpdate.Name);
                    var mapInfo = new MapInfoModel
                    {
                        ID = mapUpdate.Id,
                        MapInfo = new MapMetadata
                        {
                            Name = mapUpdate.Name,
                            Hidden = mapUpdate.Hidden,
                            Priority = mapUpdate.Priority
                        },
                        Size = 0
                    };
                    OnMapUpdated?.Invoke(mapInfo);
                }
            }
            else if (eventType == "mapDelete")
            {
                // Map deletion event
                var mapDelete = JsonSerializer.Deserialize<MapDeleteDto>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (mapDelete != null)
                {
                    _logger.LogInformation("Received map delete event: {Id}", mapDelete.Id);
                    OnMapDeleted?.Invoke(mapDelete.Id);
                }
            }
            else if (eventType == "mapRevision")
            {
                // Map revision event (cache busting)
                var mapRevision = JsonSerializer.Deserialize<MapRevisionDto>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (mapRevision != null)
                {
                    _logger.LogInformation("Received map revision event: Map {MapId} -> Revision {Revision}", mapRevision.MapId, mapRevision.Revision);
                    OnMapRevision?.Invoke(mapRevision.MapId, mapRevision.Revision);
                }
            }
            else
            {
                // Default message event (tile updates)
                var updates = JsonSerializer.Deserialize<List<TileUpdate>>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (updates != null && updates.Count > 0)
                {
                    _logger.LogDebug("Received {Count} tile updates", updates.Count);
                    OnTileUpdate?.Invoke(updates);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SSE event: {EventType}, Data: {Data}", eventType, data);
        }
    }

    // DTOs for SSE events
    private class MapUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Hidden { get; set; }
        public int Priority { get; set; }
    }

    private class MapDeleteDto
    {
        public int Id { get; set; }
    }

    private class MapRevisionDto
    {
        public int MapId { get; set; }
        public int Revision { get; set; }
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        _cts?.Cancel();

        if (_readTask != null)
        {
            try
            {
                await _readTask;
            }
            catch
            {
                // Ignore exceptions during cleanup
            }
        }

        await CleanupConnectionAsync();
        _cts?.Dispose();
    }
}
