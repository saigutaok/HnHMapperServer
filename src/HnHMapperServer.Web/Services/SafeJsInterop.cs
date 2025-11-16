using Microsoft.JSInterop;

namespace HnHMapperServer.Web.Services;

/// <summary>
/// Provides safe wrappers around JS Interop that swallow benign exceptions during SignalR reconnects.
/// Prevents circuit crashes when Blazor Server briefly loses connection ("rejoining server").
/// </summary>
public class SafeJsInterop
{
    private readonly ILogger<SafeJsInterop> _logger;
    private readonly IJSRuntime _jsRuntime;

    public SafeJsInterop(ILogger<SafeJsInterop> logger, IJSRuntime jsRuntime)
    {
        _logger = logger;
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Safely invoke a void JS method, swallowing benign disconnect exceptions.
    /// </summary>
    /// <param name="module">JS module reference</param>
    /// <param name="identifier">JS function name</param>
    /// <param name="args">Function arguments</param>
    /// <returns>True if successful, false if swallowed exception</returns>
    public async Task<bool> InvokeVoidSafeAsync(IJSObjectReference? module, string identifier, params object?[]? args)
    {
        if (module == null)
        {
            try { await _jsRuntime.InvokeVoidAsync("console.error", $"[SafeJsInterop] ERROR: module is null for {identifier}"); } catch { }
            _logger.LogDebug("InvokeVoidSafe: module is null for {Identifier}", identifier);
            return false;
        }

        try
        {
            await module.InvokeVoidAsync(identifier, args);
            return true;
        }
        catch (TaskCanceledException ex)
        {
            try { await _jsRuntime.InvokeVoidAsync("console.warn", $"[SafeJsInterop] {identifier} - TaskCanceledException"); } catch { }
            _logger.LogDebug("InvokeVoidSafe: TaskCanceledException for {Identifier} (reconnecting)", identifier);
            return false;
        }
        catch (JSDisconnectedException ex)
        {
            try { await _jsRuntime.InvokeVoidAsync("console.warn", $"[SafeJsInterop] {identifier} - JSDisconnectedException"); } catch { }
            _logger.LogDebug("InvokeVoidSafe: JSDisconnectedException for {Identifier} (circuit lost)", identifier);
            return false;
        }
        catch (ObjectDisposedException ex)
        {
            try { await _jsRuntime.InvokeVoidAsync("console.warn", $"[SafeJsInterop] {identifier} - ObjectDisposedException"); } catch { }
            _logger.LogDebug("InvokeVoidSafe: ObjectDisposedException for {Identifier} (disposed)", identifier);
            return false;
        }
        catch (Exception ex)
        {
            try { await _jsRuntime.InvokeVoidAsync("console.error", $"[SafeJsInterop] {identifier} - EXCEPTION: {ex.GetType().Name}: {ex.Message}\nStack: {ex.StackTrace}"); } catch { }
            _logger.LogWarning(ex, "InvokeVoidSafe: Unexpected exception for {Identifier}", identifier);
            return false;
        }
    }

    /// <summary>
    /// Safely invoke a JS method that returns a value, swallowing benign disconnect exceptions.
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="module">JS module reference</param>
    /// <param name="identifier">JS function name</param>
    /// <param name="args">Function arguments</param>
    /// <returns>Result value or default(T) if exception swallowed</returns>
    public async Task<T?> InvokeSafeAsync<T>(IJSObjectReference? module, string identifier, params object?[]? args)
    {
        if (module == null)
        {
            _logger.LogDebug("InvokeSafe: module is null for {Identifier}", identifier);
            return default;
        }

        try
        {
            return await module.InvokeAsync<T>(identifier, args);
        }
        catch (TaskCanceledException)
        {
            // Blazor was reconnecting; circuit was cancelled
            _logger.LogDebug("InvokeSafe: TaskCanceledException for {Identifier} (reconnecting)", identifier);
            return default;
        }
        catch (JSDisconnectedException)
        {
            // JS runtime disconnected (circuit lost)
            _logger.LogDebug("InvokeSafe: JSDisconnectedException for {Identifier} (circuit lost)", identifier);
            return default;
        }
        catch (ObjectDisposedException)
        {
            // Component was disposed while operation was in progress
            _logger.LogDebug("InvokeSafe: ObjectDisposedException for {Identifier} (disposed)", identifier);
            return default;
        }
        catch (Exception ex)
        {
            // Unexpected exception - log as warning
            _logger.LogWarning(ex, "InvokeSafe: Unexpected exception for {Identifier}", identifier);
            return default;
        }
    }
}

