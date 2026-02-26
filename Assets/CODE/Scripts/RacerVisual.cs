using UnityEngine;
using Utilities.Extensions;
using Sirenix.OdinInspector;
using MoreMountains.Feedbacks;

/// Represents a single racer's visual model (bike + rider) in the race.
public class RacerVisual : MonoBehaviour
{
    [Title("References")]
    public RaceParticipant Participant;
    public GameObject BikeModel;
    public GameObject RiderModel;

    [Title("Movement Settings")]
    [SerializeField] private float lookAheadDistance = 10f;  // How far ahead to look for rotation
    [SerializeField] private float rotationSpeed = 8f;       // How fast bike rotates
    [SerializeField] private float positionSmoothTime = 0.1f; // Position smoothing

    [Title("Banking/Leaning")]
    [SerializeField] private float maxBankAngle = 25f;       // Max lean angle on turns
    [SerializeField] private float bankSpeed = 3f;           // How fast bike leans

    private DriveRoute _route;
    private float[] _waypointOffsets; // Lane offset for each waypoint segment

    private Vector3 _currentVelocity;
    private float _currentBankAngle;
    private Quaternion _targetRotation;

    public void Initialize(RaceParticipant participant, DriveRoute route, BikeData bikeData)
    {
        Participant = participant;
        _route = route;

        // Generate random offsets for each waypoint segment
        if (_route != null)
        {
            _waypointOffsets = new float[_route.GetWaypointCount()];
            for (int i = 0; i < _waypointOffsets.Length; i++)
            {
                _waypointOffsets[i] = Random.Range(-_route.RouteWidth / 2f, _route.RouteWidth / 2f);
            }
        }

        // Spawn bike model
        if (bikeData?.BikePrefab)
        {
            BikeModel = Instantiate(bikeData.BikePrefab, transform);
            BikeModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        // Spawn rider model (random customer visual)
        var riderPrefab = CustomerManager.Instance.CustomersVisuals.GetRandom();
        if (riderPrefab)
        {
            RiderModel = Instantiate(riderPrefab.gameObject, transform);
            RiderModel.transform.SetLocalPositionAndRotation(Vector3.up * 1.8f, Quaternion.identity);
        }
    }

    public void UpdatePosition()
    {
        if (_route == null || _route.GetWaypointCount() < 2) return;

        // Normalize progress to route (0 = start, 1 = finish)
        float normalizedProgress = _route.RouteLength > 0 ? Mathf.Clamp01(Participant.Progress / _route.RouteLength) : 0f;
        if (normalizedProgress >= 1.0f)
        {
            ToggleFeedbacks(false);
            return; // Stay at finish line, don't process further
        }

        // Get current position with lane offset
        Vector3 targetPosition = GetPositionOnRouteWithOffset(normalizedProgress, out int currentSegment);

        // Smooth position movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, positionSmoothTime);

        if (normalizedProgress < 0.99f)
        {
            // Get look-ahead point for smooth rotation
            Vector3 lookAheadPoint = GetLookAheadPoint(normalizedProgress, lookAheadDistance);

            // Calculate rotation towards look-ahead point
            UpdateRotationAndBanking(lookAheadPoint, currentSegment);
        }
    }

    private Vector3 GetPositionOnRouteWithOffset(float normalizedProgress, out int currentSegment)
    {
        int waypointCount = _route.GetWaypointCount();
        float exactWaypoint = normalizedProgress * (waypointCount - 1);

        currentSegment = Mathf.FloorToInt(exactWaypoint);
        float t = exactWaypoint - currentSegment;

        // Clamp to valid waypoint range
        currentSegment = Mathf.Clamp(currentSegment, 0, waypointCount - 2);
        int nextWaypoint = currentSegment + 1;

        // Interpolate between waypoints
        Vector3 start = _route.GetWaypoint(currentSegment);
        Vector3 end = _route.GetWaypoint(nextWaypoint);
        Vector3 basePosition = Vector3.Lerp(start, end, t);

        // Apply lane offset perpendicular to the path
        Vector3 pathDirection = (end - start).normalized;
        Vector3 rightOffset = Vector3.Cross(Vector3.up, pathDirection).normalized;

        // Interpolate offset between waypoints for smoother lane transitions
        float currentOffset = _waypointOffsets[currentSegment];
        float nextOffset = _waypointOffsets[nextWaypoint];
        float interpolatedOffset = Mathf.Lerp(currentOffset, nextOffset, t);

        return basePosition + (rightOffset * interpolatedOffset);
    }

    private Vector3 GetLookAheadPoint(float currentProgress, float distance)
    {
        // Calculate how much further along the route to look based on distance
        float lookAheadProgress = currentProgress + (distance / _route.RouteLength);
        lookAheadProgress = Mathf.Clamp01(lookAheadProgress);

        // If we're at the end, just look slightly ahead
        if (lookAheadProgress >= 0.99f)
        {
            lookAheadProgress = Mathf.Min(currentProgress + 0.01f, 1f);
        }

        return GetPositionOnRouteWithOffset(lookAheadProgress, out _);
    }

    private void UpdateRotationAndBanking(Vector3 lookAheadPoint, int currentSegment)
    {
        // Calculate direction to look-ahead point
        Vector3 directionToTarget = lookAheadPoint - transform.position;

        // Only rotate if we have a significant direction (prevents flickering)
        if (directionToTarget.sqrMagnitude < 0.01f) return; // Too close to target, keep current rotation

        // Project direction onto horizontal plane
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        // Calculate target rotation (horizontal only)
        Quaternion targetYawRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        // Calculate turn angle for banking
        float turnAngle = CalculateTurnAngle(currentSegment);

        // Smooth bank angle based on turn sharpness
        float targetBankAngle = Mathf.Clamp(turnAngle * maxBankAngle, -maxBankAngle, maxBankAngle);
        _currentBankAngle = Mathf.Lerp(_currentBankAngle, targetBankAngle, Time.deltaTime * bankSpeed);

        // Apply banking (roll) to the rotation
        Quaternion bankRotation = Quaternion.Euler(0, 0, -_currentBankAngle);
        _targetRotation = targetYawRotation * bankRotation;

        // Smooth rotation interpolation
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * rotationSpeed);
    }

    private float CalculateTurnAngle(int currentSegment)
    {
        int waypointCount = _route.GetWaypointCount();

        // Need at least 3 points to calculate turn angle
        if (currentSegment >= waypointCount - 2) return 0f;

        Vector3 prevPoint = currentSegment > 0 ? _route.GetWaypoint(currentSegment - 1) : _route.GetWaypoint(currentSegment);
        Vector3 currentPoint = _route.GetWaypoint(currentSegment);
        Vector3 nextPoint = _route.GetWaypoint(currentSegment + 1);
        Vector3 afterNextPoint = currentSegment + 2 < waypointCount ? _route.GetWaypoint(currentSegment + 2) : _route.GetWaypoint(currentSegment + 1);

        // Calculate angles between path segments
        Vector3 dir1 = (currentPoint - prevPoint).normalized;
        Vector3 dir2 = (nextPoint - currentPoint).normalized;
        Vector3 dir3 = (afterNextPoint - nextPoint).normalized;

        // Average the turn directions for smoother banking
        Vector3 avgDir1 = Vector3.Lerp(dir1, dir2, 0.5f).normalized;
        Vector3 avgDir2 = Vector3.Lerp(dir2, dir3, 0.5f).normalized;

        // Calculate signed angle (positive = right turn, negative = left turn)
        float angle = Vector3.SignedAngle(avgDir1, avgDir2, Vector3.up);

        // Normalize to -1 to 1 range
        return Mathf.Clamp(angle / 90f, -1f, 1f);
    }

    public void ToggleFeedbacks(bool toggle)
    {
        if (BikeModel.TryGetComponentInChildren(out MMF_Player feedbacks, true))
        {
            feedbacks.gameObject.SetActive(toggle);
            if (toggle) feedbacks.PlayFeedbacks();
            else feedbacks.StopFeedbacks();
        }
    }

    public float GetCurrentSpeed() => _currentVelocity.magnitude;
    public float GetBankAngle() => _currentBankAngle;
}