using System.Text;
using Serilog;
using VRCX.Core.DiscordRpc;
using VRCX.Core.DiscordRpc.Models;
using VRCX.Core.Shared;

namespace VRCX.Core.Services;

public sealed class DiscordService : IDisposable
{
    private readonly ILogger _logger = Log.ForContext<DiscordService>();

    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(10);
    private bool _isFirstRetry = true;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly DiscordRpcActivity _activity = new();
    private string? _discordAppId;

    private bool _isActive;
    private CancellationTokenSource? _cts;

    private const string VrcxUrl = "https://vrcx.app";

    public void Start()
    {
        if (_cts is not null)
            throw new InvalidOperationException("Discord Service is already started.");

        _cts = new CancellationTokenSource();
        _ = Task.Factory.StartNew(() => CoreLoopAsync(_cts.Token), TaskCreationOptions.LongRunning);
    }

    public async Task StopAsync()
    {
        if (_cts is not null)
            await _cts.CancelAsync();

        _cts?.Dispose();
        _semaphore.Dispose();
    }

    private async Task CoreLoopAsync(CancellationToken cancellationToken)
    {
        SimpleDiscordRpcClient? client = null;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_discordAppId is null)
                {
                    if (client is not null)
                    {
                        if (client.IsReady)
                            await client.ClearActivityAsync();

                        client.Dispose();
                        client = null;
                    }

                    await Task.Delay(UpdateInterval, cancellationToken);
                    continue;
                }

                if (client is null || client.ClientId != _discordAppId)
                {
                    if (client is not null)
                    {
                        client.Dispose();
                        client = null;
                    }

                    if (!_isActive)
                    {
                        await Task.Delay(UpdateInterval, cancellationToken);
                        continue;
                    }

                    client = new SimpleDiscordRpcClient(_discordAppId);
                    try
                    {
                        await client.ConnectAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        if (_isFirstRetry)
                        {
                            _logger.Warning(ex, "Failed to connect to Discord RPC, will retry slightly later");
                            _isFirstRetry = false;
                        }
                        else
                        {
                            _logger.Verbose(ex, "Failed to connect to Discord RPC, will retry");
                        }

                        client.Dispose();
                        client = null;

                        await Task.Delay(ReconnectDelay, cancellationToken);
                        continue;
                    }
                }

                if (!client.IsReady)
                {
                    _logger.Warning("Discord RPC client is not ready, will reconnect");
                    client.Dispose();
                    client = null;
                    await Task.Delay(ReconnectDelay, cancellationToken);
                    continue;
                }

                if (!_isActive)
                {
                    await Task.Delay(UpdateInterval, cancellationToken);
                }

                try
                {
                    await client.SetActivityAsync(_activity);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to set Discord RPC activity, will attempt to reconnect");
                    client.Dispose();
                    client = null;

                    await Task.Delay(ReconnectDelay, cancellationToken);
                    continue;
                }

                await Task.Delay(UpdateInterval, cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.Verbose(ex, "Discord Service loop cancellation requested");
                // ignored
                break;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error occurred in Discord Service loop, will attempt to reconnect");
                client?.Dispose();
                client = null;

                await Task.Delay(ReconnectDelay, cancellationToken);
            }
        }

        try
        {
            if (client?.IsReady is true)
            {
                await client.ClearActivityAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to clear Discord RPC activity during shutdown");
        }

        client?.Dispose();
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }

    public bool SetActive(bool active)
    {
        _isActive = active;
        return _isActive;
    }

    public async Task SetAssetsAsync(
        string details,
        string state,
        string detailsUrl,
        string largeKey,
        string largeText,
        string smallKey,
        string smallText,
        double startUnixMilliseconds,
        double endUnixMilliseconds,
        string partyId,
        int partySize,
        int partyMax,
        string buttonText,
        string buttonUrl,
        string appId,
        int activityType,
        int statusDisplayType)
    {
        using (await SimpleSemaphoreSlimLockScope.WaitAsync(_semaphore))
        {
            _logger.Verbose("Updating Discord RPC Activity");

            if (string.IsNullOrEmpty(largeKey) &&
                string.IsNullOrEmpty(smallKey))
            {
                _activity.Assets = null;
                _activity.Party = null;
                _activity.Timestamps = null;
                return;
            }

            _activity.Details = LimitByteLength(details, 127);
            _activity.DetailsUrl = !string.IsNullOrEmpty(detailsUrl) ? detailsUrl : null;
            // _presence.StateUrl
            _activity.State = !string.IsNullOrWhiteSpace(state) ? LimitByteLength(state, 127) : "Test";
            _activity.Assets ??= new DiscordRpcActivityAssets();

            _activity.Assets.LargeImageKey = largeKey;
            _activity.Assets.LargeImageText = largeText;
            _activity.Assets.LargeImageUrl = VrcxUrl;

            _activity.Assets.SmallImageKey = smallKey;
            _activity.Assets.SmallImageText = smallText;
            // m_Presence.Assets.SmallImageUrl

            if (startUnixMilliseconds == 0)
            {
                _activity.Timestamps = null;
            }
            else
            {
                _activity.Timestamps ??= new DiscordRpcActivityTimestamps();
                _activity.Timestamps.StartUnixMilliseconds = (ulong)startUnixMilliseconds;
                if (endUnixMilliseconds == 0)
                    _activity.Timestamps.EndUnixMilliseconds = null;
                else
                    _activity.Timestamps.EndUnixMilliseconds = (ulong)endUnixMilliseconds;
            }

            if (partyMax == 0)
            {
                _activity.Party = null;
            }
            else
            {
                _activity.Party ??= new DiscordRpcActivityParty();
                _activity.Party.Id = partyId;
                _activity.Party.Current = partySize;
                _activity.Party.Max = partyMax;
            }

            _activity.Type = (DiscordRpcActivityType)activityType;
            _activity.StatusDisplayType = (DiscordRpcActivityStatusDisplayType)statusDisplayType;

            _activity.Buttons = null;
            if (!string.IsNullOrEmpty(buttonUrl))
            {
                _activity.Buttons =
                [
                    new DiscordRpcActivityButton { Label = buttonText, Url = buttonUrl }
                ];
            }

            _discordAppId = appId;
        }
    }


    // https://stackoverflow.com/questions/1225052/best-way-to-shorten-utf8-string-based-on-byte-length
    private static string LimitByteLength(string? str, int maxBytesLength)
    {
        if (str == null)
            return string.Empty;
        var bytesArr = Encoding.UTF8.GetBytes(str);
        var bytesToRemove = 0;
        var lastIndexInString = str.Length - 1;
        while (bytesArr.Length - bytesToRemove > maxBytesLength)
        {
            bytesToRemove += Encoding.UTF8.GetByteCount(new[] { str[lastIndexInString] });
            --lastIndexInString;
        }

        return Encoding.UTF8.GetString(bytesArr, 0, bytesArr.Length - bytesToRemove);
    }
}