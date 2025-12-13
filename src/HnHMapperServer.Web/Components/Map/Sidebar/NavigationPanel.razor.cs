using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

namespace HnHMapperServer.Web.Components.Map.Sidebar;

public partial class NavigationPanel
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public RouteResult? Route { get; set; }
    [Parameter] public EventCallback OnClearRoute { get; set; }
    [Parameter] public EventCallback<int> OnJumpToRoad { get; set; }

    private string FormatDistance(int distance)
    {
        if (distance < 1000)
            return $"{distance} px";
        return $"{distance / 1000.0:F1}k px";
    }

    private string GetStepStyle(int index)
    {
        if (Route?.Segments == null || Route.Segments.Count == 0)
            return "background: #1976D2; color: white; font-size: 12px;";

        if (index == 0)
            return "background: #22C55E; color: white; font-size: 12px;"; // Green for first
        if (index == Route.Segments.Count - 1)
            return "background: #EF4444; color: white; font-size: 12px;"; // Red for last
        return "background: #1976D2; color: white; font-size: 12px;"; // Blue for middle
    }
}

/// <summary>
/// Route calculation result from JavaScript
/// </summary>
public class RouteResult
{
    [JsonPropertyName("segments")]
    public List<RouteSegment> Segments { get; set; } = new();

    [JsonPropertyName("totalDistance")]
    public int TotalDistance { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("partial")]
    public bool Partial { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// A single segment (road) in the route
/// </summary>
public class RouteSegment
{
    [JsonPropertyName("roadId")]
    public int RoadId { get; set; }

    [JsonPropertyName("roadName")]
    public string RoadName { get; set; } = "";

    [JsonPropertyName("jumpDistance")]
    public int JumpDistance { get; set; }

    [JsonPropertyName("roadLength")]
    public int RoadLength { get; set; }

    [JsonPropertyName("finalJumpDistance")]
    public int FinalJumpDistance { get; set; }
}
