using HnHMapperServer.Core.Models;
using HnHMapperServer.Core.DTOs;
using HnHMapperServer.Services.Interfaces;
using System.Threading.Channels;
using System.Collections.Concurrent;

namespace HnHMapperServer.Services.Services;

public class UpdateNotificationService : IUpdateNotificationService
{
    private readonly ConcurrentBag<Channel<TileData>> _tileChannels = new();
    private readonly ConcurrentBag<Channel<MergeDto>> _mergeChannels = new();
    private readonly ConcurrentBag<Channel<MapInfo>> _mapUpdateChannels = new();
    private readonly ConcurrentBag<Channel<int>> _mapDeleteChannels = new();
    private readonly ConcurrentBag<Channel<MapRevisionDto>> _mapRevisionChannels = new();
    private readonly ConcurrentBag<Channel<CustomMarkerEventDto>> _customMarkerCreatedChannels = new();
    private readonly ConcurrentBag<Channel<CustomMarkerEventDto>> _customMarkerUpdatedChannels = new();
    private readonly ConcurrentBag<Channel<CustomMarkerDeleteEventDto>> _customMarkerDeletedChannels = new();
    private readonly ConcurrentBag<Channel<CharacterDeltaDto>> _characterDeltaChannels = new();
    private readonly ConcurrentBag<Channel<PingEventDto>> _pingCreatedChannels = new();
    private readonly ConcurrentBag<Channel<PingDeleteEventDto>> _pingDeletedChannels = new();

    public ChannelReader<TileData> SubscribeToTileUpdates()
    {
        var channel = Channel.CreateUnbounded<TileData>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _tileChannels.Add(channel);
        return channel.Reader;
    }

    public ChannelReader<MergeDto> SubscribeToMergeUpdates()
    {
        var channel = Channel.CreateUnbounded<MergeDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _mergeChannels.Add(channel);
        return channel.Reader;
    }

    public ChannelReader<MapInfo> SubscribeToMapUpdates()
    {
        var channel = Channel.CreateUnbounded<MapInfo>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _mapUpdateChannels.Add(channel);
        return channel.Reader;
    }

    public ChannelReader<int> SubscribeToMapDeletes()
    {
        var channel = Channel.CreateUnbounded<int>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _mapDeleteChannels.Add(channel);
        return channel.Reader;
    }

    public ChannelReader<MapRevisionDto> SubscribeToMapRevisions()
    {
        var channel = Channel.CreateUnbounded<MapRevisionDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _mapRevisionChannels.Add(channel);
        return channel.Reader;
    }

    public void NotifyTileUpdate(TileData tileData)
    {
        var channelsToRemove = new ConcurrentBag<Channel<TileData>>();

        foreach (var channel in _tileChannels)
        {
            if (!channel.Writer.TryWrite(tileData))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public void NotifyMapMerge(int fromMapId, int toMapId, Coord shift, string tenantId)
    {
        var merge = new MergeDto
        {
            From = fromMapId,
            To = toMapId,
            Shift = shift,
            TenantId = tenantId
        };

        var channelsToRemove = new ConcurrentBag<Channel<MergeDto>>();

        foreach (var channel in _mergeChannels)
        {
            if (!channel.Writer.TryWrite(merge))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public void NotifyMapUpdated(MapInfo mapInfo)
    {
        var channelsToRemove = new ConcurrentBag<Channel<MapInfo>>();

        foreach (var channel in _mapUpdateChannels)
        {
            if (!channel.Writer.TryWrite(mapInfo))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public void NotifyMapDeleted(int mapId)
    {
        var channelsToRemove = new ConcurrentBag<Channel<int>>();

        foreach (var channel in _mapDeleteChannels)
        {
            if (!channel.Writer.TryWrite(mapId))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public void NotifyMapRevision(int mapId, int revision)
    {
        var dto = new MapRevisionDto
        {
            MapId = mapId,
            Revision = revision
        };

        var channelsToRemove = new ConcurrentBag<Channel<MapRevisionDto>>();

        foreach (var channel in _mapRevisionChannels)
        {
            if (!channel.Writer.TryWrite(dto))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public ChannelReader<CustomMarkerEventDto> SubscribeToCustomMarkerCreated()
    {
        var channel = Channel.CreateUnbounded<CustomMarkerEventDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _customMarkerCreatedChannels.Add(channel);
        return channel.Reader;
    }

    public ChannelReader<CustomMarkerEventDto> SubscribeToCustomMarkerUpdated()
    {
        var channel = Channel.CreateUnbounded<CustomMarkerEventDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _customMarkerUpdatedChannels.Add(channel);
        return channel.Reader;
    }

    public ChannelReader<CustomMarkerDeleteEventDto> SubscribeToCustomMarkerDeleted()
    {
        var channel = Channel.CreateUnbounded<CustomMarkerDeleteEventDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _customMarkerDeletedChannels.Add(channel);
        return channel.Reader;
    }

    public void NotifyCustomMarkerCreated(CustomMarkerEventDto marker)
    {
        var channelsToRemove = new ConcurrentBag<Channel<CustomMarkerEventDto>>();

        foreach (var channel in _customMarkerCreatedChannels)
        {
            if (!channel.Writer.TryWrite(marker))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public void NotifyCustomMarkerUpdated(CustomMarkerEventDto marker)
    {
        var channelsToRemove = new ConcurrentBag<Channel<CustomMarkerEventDto>>();

        foreach (var channel in _customMarkerUpdatedChannels)
        {
            if (!channel.Writer.TryWrite(marker))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public void NotifyCustomMarkerDeleted(CustomMarkerDeleteEventDto deleteEvent)
    {
        var channelsToRemove = new ConcurrentBag<Channel<CustomMarkerDeleteEventDto>>();

        foreach (var channel in _customMarkerDeletedChannels)
        {
            if (!channel.Writer.TryWrite(deleteEvent))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public ChannelReader<CharacterDeltaDto> SubscribeToCharacterDelta()
    {
        // Use unbounded channel to prevent dropping deltas
        // Each SSE connection filters by tenant ID, so cross-tenant deltas
        // will be skipped but not dropped
        var channel = Channel.CreateUnbounded<CharacterDeltaDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _characterDeltaChannels.Add(channel);
        return channel.Reader;
    }

    public void NotifyCharacterDelta(CharacterDeltaDto delta)
    {
        var channelsToRemove = new ConcurrentBag<Channel<CharacterDeltaDto>>();

        foreach (var channel in _characterDeltaChannels)
        {
            if (!channel.Writer.TryWrite(delta))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public ChannelReader<PingEventDto> SubscribeToPingCreated()
    {
        var channel = Channel.CreateUnbounded<PingEventDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _pingCreatedChannels.Add(channel);
        return channel.Reader;
    }

    public ChannelReader<PingDeleteEventDto> SubscribeToPingDeleted()
    {
        var channel = Channel.CreateUnbounded<PingDeleteEventDto>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _pingDeletedChannels.Add(channel);
        return channel.Reader;
    }

    public void NotifyPingCreated(PingEventDto ping)
    {
        var channelsToRemove = new ConcurrentBag<Channel<PingEventDto>>();

        foreach (var channel in _pingCreatedChannels)
        {
            if (!channel.Writer.TryWrite(ping))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }

    public void NotifyPingDeleted(PingDeleteEventDto deleteEvent)
    {
        var channelsToRemove = new ConcurrentBag<Channel<PingDeleteEventDto>>();

        foreach (var channel in _pingDeletedChannels)
        {
            if (!channel.Writer.TryWrite(deleteEvent))
            {
                // Channel is likely closed or full, mark for removal
                channelsToRemove.Add(channel);
            }
        }

        // Clean up dead channels
        foreach (var deadChannel in channelsToRemove)
        {
            deadChannel.Writer.TryComplete();
        }
    }
}
