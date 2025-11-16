using HnHMapperServer.Core.Models;
using HnHMapperServer.Core.DTOs;
using System.Threading.Channels;

namespace HnHMapperServer.Services.Interfaces;

public interface IUpdateNotificationService
{
    /// <summary>
    /// Subscribes to tile update notifications
    /// </summary>
    ChannelReader<TileData> SubscribeToTileUpdates();

    /// <summary>
    /// Subscribes to map merge notifications
    /// </summary>
    ChannelReader<MergeDto> SubscribeToMergeUpdates();

    /// <summary>
    /// Subscribes to map metadata update notifications (rename, hidden, priority changes)
    /// </summary>
    ChannelReader<MapInfo> SubscribeToMapUpdates();

    /// <summary>
    /// Subscribes to map deletion notifications
    /// </summary>
    ChannelReader<int> SubscribeToMapDeletes();

    /// <summary>
    /// Subscribes to map revision notifications (for cache busting)
    /// </summary>
    ChannelReader<MapRevisionDto> SubscribeToMapRevisions();

    /// <summary>
    /// Notifies all subscribers of a tile update
    /// </summary>
    void NotifyTileUpdate(TileData tileData);

    /// <summary>
    /// Notifies all subscribers of a map merge
    /// </summary>
    void NotifyMapMerge(int fromMapId, int toMapId, Coord shift, string tenantId);

    /// <summary>
    /// Notifies all subscribers of a map metadata update (rename, hidden, priority)
    /// </summary>
    void NotifyMapUpdated(MapInfo mapInfo);

    /// <summary>
    /// Notifies all subscribers of a map deletion
    /// </summary>
    void NotifyMapDeleted(int mapId);

    /// <summary>
    /// Notifies all subscribers of a map revision update (for cache busting)
    /// </summary>
    void NotifyMapRevision(int mapId, int revision);

    /// <summary>
    /// Subscribes to custom marker creation notifications
    /// </summary>
    ChannelReader<CustomMarkerEventDto> SubscribeToCustomMarkerCreated();

    /// <summary>
    /// Subscribes to custom marker update notifications
    /// </summary>
    ChannelReader<CustomMarkerEventDto> SubscribeToCustomMarkerUpdated();

    /// <summary>
    /// Subscribes to custom marker deletion notifications
    /// </summary>
    ChannelReader<CustomMarkerDeleteEventDto> SubscribeToCustomMarkerDeleted();

    /// <summary>
    /// Notifies all subscribers of a custom marker creation
    /// </summary>
    void NotifyCustomMarkerCreated(CustomMarkerEventDto marker);

    /// <summary>
    /// Notifies all subscribers of a custom marker update
    /// </summary>
    void NotifyCustomMarkerUpdated(CustomMarkerEventDto marker);

    /// <summary>
    /// Notifies all subscribers of a custom marker deletion
    /// </summary>
    void NotifyCustomMarkerDeleted(CustomMarkerDeleteEventDto deleteEvent);

    /// <summary>
    /// Subscribes to character delta notifications (incremental updates)
    /// </summary>
    ChannelReader<CharacterDeltaDto> SubscribeToCharacterDelta();

    /// <summary>
    /// Notifies all subscribers of character deltas (incremental changes)
    /// </summary>
    void NotifyCharacterDelta(CharacterDeltaDto delta);

    /// <summary>
    /// Subscribes to ping creation notifications
    /// </summary>
    ChannelReader<PingEventDto> SubscribeToPingCreated();

    /// <summary>
    /// Subscribes to ping deletion notifications
    /// </summary>
    ChannelReader<PingDeleteEventDto> SubscribeToPingDeleted();

    /// <summary>
    /// Notifies all subscribers of a ping creation
    /// </summary>
    void NotifyPingCreated(PingEventDto ping);

    /// <summary>
    /// Notifies all subscribers of a ping deletion
    /// </summary>
    void NotifyPingDeleted(PingDeleteEventDto deleteEvent);
}
