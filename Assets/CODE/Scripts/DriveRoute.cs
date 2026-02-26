using System.Linq;
using UnityEngine;
using Utilities.Extensions;

public class DriveRoute : MonoBehaviour
{
    [Header("Route Settings")]
    public Transform[] Waypoints;
    public float RouteLength = -1f; // In meters
    public float RouteWidth = 5f;

    [Header("Visuals")]
    public bool ShowGizmos = true;
    public Color RouteColor = Color.cyan;

    private void Awake()
    {
        if (RouteLength <= 0) RouteLength = CalculateRouteDistance();
    }

    public Vector3 GetWaypoint(int index)
    {
        return Waypoints.IsNullOrEmpty() ? Vector3.zero : Waypoints[Mathf.Clamp(index, 0, Waypoints.Length - 1)].position;
    }

    public int GetWaypointCount()
    {
        return Waypoints?.Length ?? 0;
    }

    public float CalculateRouteDistance()
    {
        if (GetWaypointCount() < 2) return 1f;

        float distance = 0f;
        for (int i = 0; i < GetWaypointCount() - 1; i++)
        {
            Vector3 current = GetWaypoint(i);
            Vector3 next = GetWaypoint(i + 1);
            distance += Vector3.Distance(current, next);
        }

        return distance > 0 ? distance : 1f;
    }

    private void OnDrawGizmos()
    {
        if (!ShowGizmos || Waypoints == null || Waypoints.Length < 2) return;

        Vector3[] centerPoints = Waypoints.Select(p => p.position).ToArray();

        Gizmos.color = RouteColor;
        
        Gizmos.DrawLineStrip(centerPoints, true);
        foreach (Vector3 point in centerPoints) Gizmos.DrawSphere(point, 0.5f);

        if (RouteWidth > 0f)
        {
            Vector3[] leftBoundary = new Vector3[Waypoints.Length];
            Vector3[] rightBoundary = new Vector3[Waypoints.Length];

            for (int i = 0; i < Waypoints.Length; i++)
            {
                Vector3 prev = Waypoints[i == 0 ? Waypoints.Length - 1 : i - 1].position;
                Vector3 next = Waypoints[(i + 1) % Waypoints.Length].position;
                Vector3 direction = (next - prev).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;

                leftBoundary[i] = centerPoints[i] - (right * (RouteWidth / 2f));
                rightBoundary[i] = centerPoints[i] + (right * (RouteWidth / 2f));
            }

            Gizmos.color = RouteColor * 0.5f;
            Gizmos.DrawLineStrip(leftBoundary, true);
            Gizmos.DrawLineStrip(rightBoundary, true);
        }
    }
}